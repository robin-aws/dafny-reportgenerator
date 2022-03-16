
include "../libraries/src/Wrappers.dfy"
include "../libraries/src/Collections/Sequences/Seq.dfy"

include "Externs.dfy"

module OptionsParsing {

  import opened Wrappers
  
  import Externs
  import Seq
  
  class Box<T> {
    var value: T
  }

  class CLICommand {
    var options: seq<CLIOption>

    method Positional(name: string, description: string) returns (option: CLIOption)

  }

  trait CLIOption {
    
    const longName: Option<string>
    
    predicate method RequiresArgument()

    method Parse(argument: string) returns (result: Result<(), string>)
      modifies this

    function method HelpText(): string
  }

  class Positional extends CLIOption {

    var values: seq<string>
    const helpText: string

    constructor(placeholder: string, description: string) {
      longName := None;
      helpText := placeholder + ": " + description;
    }

    predicate method RequiresArgument() {
      false
    }

    method Parse(argument: string) returns (result: Result<(), string>)
      modifies this
    {
      values := values + [argument];
      return Success(());
    }

    function method HelpText(): string {
      helpText
    }
  }

  class SingleNaturalNumber extends CLIOption {
    
    var valueOption: Option<nat>
    const helpText: string

    constructor(name: string, description: string) {
      longName := Some(name);
      helpText := name + ": " + description;
    }

    predicate method RequiresArgument() {
      true
    }

    method Parse(argument: string) returns (result: Result<(), string>)
      modifies this
    {
      :- Need(valueOption.None?, longName.value + " can only be provided once");
      var value :- Externs.ParseNat(argument);
      valueOption := Some(value);
    }

    function method HelpText(): string {
      helpText
    }
  }

  function method UsageText(options: seq<CLIOption>): string {
    "Usage:\n\n" + Seq.Flatten(Seq.Map((o: CLIOption) => o.HelpText(), options))
  }

  method ParseOptionsWithHelpText(options: seq<CLIOption>, args: seq<string>) returns (result: Result<(), string>) 
    requires !exists o, o' | o in options && o' in options :: o.longName == o'.longName
    modifies options
  {
    result := ParseOptions(options, args);
    if result.IsFailure() {
      result := Failure(result.error + "\n\n" + UsageText(options));
    }
  }

  method ParseOptions(options: seq<CLIOption>, args: seq<string>) returns (result: Result<(), string>)
    requires !exists o, o' | o in options && o' in options :: o.longName == o'.longName
    modifies options
  {
    var optionsByName := map o | o in options :: o.longName := o;
    var argIndex := 0;
    while argIndex < |args| {
      var arg := args[argIndex];
      if Some(arg) in optionsByName {
        var option := optionsByName[Some(arg)];
        :- Need(argIndex + 1 < |args|, arg + " must be followed by an argument");
        argIndex := argIndex + 1;
        var _ :- option.Parse(args[argIndex]);
      } else if None in optionsByName {
        var option := optionsByName[None];
        var _ :- option.Parse(arg);
      } else {
        return Failure("Unsupported option: " + arg);
      }
    }
    return Success(());
  }
}