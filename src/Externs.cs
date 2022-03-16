
using System.Diagnostics;
using System.Numerics;
using Dafny;
using Wrappers_Compile;
using icharseq = Dafny.ISequence<char>;
using charseq = Dafny.Sequence<char>;

namespace Externs_Compile {

  public partial class __default {
    public static ISequence<icharseq> GetCommandLineArgs() {
      var dafnyArgs = Environment.GetCommandLineArgs().Select(charseq.FromString);
      return Sequence<icharseq>.FromArray(dafnyArgs.ToArray());
    }
    
    public static void SetExitCode(int exitCode) {
      Environment.ExitCode = exitCode;
    }
    
    public static _IResult<ISequence<icharseq>, icharseq> ReadAllFileLines(icharseq dafnyPath) {
      var path = dafnyPath.ToString();
      try {
        var lines = File.ReadAllLines(path);
        var dafnyLines = Sequence<icharseq>.FromArray(lines.Select(charseq.FromString).ToArray());
        return Result<ISequence<icharseq>, icharseq>.create_Success(dafnyLines);
      } catch (Exception e) {
        return Result<ISequence<icharseq>, icharseq>.create_Failure(charseq.FromString(e.Message));
      }
    }

    public static _IResult<BigInteger, icharseq> ParseNat(icharseq dafnyString) {
      var s = dafnyString.ToString();
      try {
        return Result<BigInteger, icharseq>.create_Success(int.Parse(s));
      } catch (Exception e) {
        return Result<BigInteger, icharseq>.create_Failure(charseq.FromString(e.Message));
      }
    }

    public static icharseq NatToString(BigInteger n) {
      return charseq.FromString(n.ToString());
    }

    public static _IResult<long, icharseq> ParseDurationTicks(icharseq dafnyString) {
      var s = dafnyString.ToString();
      try {
        var timeSpan = TimeSpan.Parse(s);
        return Result<long, icharseq>.create_Success(timeSpan.Ticks);
      } catch (Exception e) {
        return Result<long, icharseq>.create_Failure(charseq.FromString(e.Message));
      }
    }
    
    public static icharseq DurationTicksToString(long ticks) {
      var timeSpan = TimeSpan.FromTicks(ticks);
      return charseq.FromString(timeSpan.ToString());
    }

    public static _IResult<icharseq, icharseq> RunCommand(ISequence<icharseq> args) {
      using var process = new Process();

      var argsArray = args.Elements;
      process.StartInfo.FileName = argsArray.First().ToString()!;
      foreach (var argument in argsArray[1..]) {
        process.StartInfo.ArgumentList.Add(argument.ToString());
      }

      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.CreateNoWindow = true;

      process.Start();
      var output = process.StandardOutput.ReadToEnd();
      var error = process.StandardError.ReadToEnd();
      process.WaitForExit();

      if (process.ExitCode == 0) {
        return Result<icharseq, icharseq>.create_Success(charseq.FromString(output));
      }

      var errorMessage = $"Command failed with exit code {process.ExitCode}. Standard error output:\n{error}";
      return Result<icharseq, icharseq>.create_Failure(charseq.FromString(errorMessage));
    }
  }
}
