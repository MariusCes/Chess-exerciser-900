using CHESSPROJ.Controllers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CHESSPROJ.Services
{
    public class StockfishService : IStockfishService, IDisposable
    {
        private readonly Process _process;
        private readonly object _lock = new();
        private int _skillLevel;

        public StockfishService(string stockfishPath)
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = stockfishPath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            _process.Start();
            SendCommand("uci");
            ReadUntil("uciok");
            SendCommand("isready");
            ReadUntil("readyok");
        }

        public void SetLevel(int level)
        {
            _skillLevel = Math.Clamp(level * 4, 0, 20);
            SendCommand($"setoption name Skill Level value {_skillLevel}");
        }

        public void SetPosition(string movesMade, string move)
        {
            var moves = string.IsNullOrWhiteSpace(movesMade) ? move : $"{movesMade} {move}";
            SendCommand($"position startpos moves {moves}");
        }

        public string GetBestMove()
        {
            SendCommand($"go depth 5");
            var output = ReadUntil("bestmove");
            var match = Regex.Match(output, @"bestmove\s+(\S+)");
            return match.Success ? match.Groups[1].Value : "e2e4"; // fallback
        }

        public bool IsMoveCorrect(string currentPosition, string move)
        {
            // Set position WITHOUT the candidate move, then run perft 1
            // perft at depth 1 lists all legal moves from current position
            if (string.IsNullOrWhiteSpace(currentPosition))
                SendCommand("position startpos");
            else
                SendCommand($"position startpos moves {currentPosition}");

            SendCommand("go perft 1");
            var output = ReadUntil("Nodes searched");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // perft output: each line is "<move>: <count>"
            foreach (var line in lines)
            {
                var parts = line.Trim().Split(':');
                if (parts.Length >= 1 && parts[0].Trim().Equals(move, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public string GetFen()
        {
            SendCommand("d");
            var output = ReadUntil("Checkers");
            var match = Regex.Match(output, @"Fen:\s*(.+)$", RegexOptions.Multiline);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            // Fallback: try without trailing space
            match = Regex.Match(output, @"Fen:\s*(.+)", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        public string GetEvalType()
        {
            SendCommand("eval");
            var output = ReadUntil("Final evaluation");
            return output.Contains("mate") ? "mate" : "cp";
        }

        public int GetEvalVal()
        {
            SendCommand("eval");
            var output = ReadUntil("Final evaluation");
            var match = Regex.Match(output, @"Final evaluation:\s+(-?\d+)");
            if (match.Success)
                return int.Parse(match.Groups[1].Value);
            return 0;
        }

        private void SendCommand(string command)
        {
            lock (_lock)
            {
                _process.StandardInput.WriteLine(command);
                _process.StandardInput.Flush();
            }
        }

        private string ReadUntil(string marker)
        {
            var output = new System.Text.StringBuilder();
            var deadline = DateTime.UtcNow.AddSeconds(30);
            lock (_lock)
            {
                string? line;
                while (DateTime.UtcNow < deadline && (line = _process.StandardOutput.ReadLine()) != null)
                {
                    output.AppendLine(line);
                    if (line.Contains(marker))
                        return output.ToString();
                }
            }
            return output.ToString();
        }

        public void Dispose()
        {
            try { SendCommand("quit"); } catch { }
            if (!_process.HasExited)
            {
                _process.WaitForExit(2000);
                if (!_process.HasExited) _process.Kill();
            }
            _process.Dispose();
        }
    }
}
