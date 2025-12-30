using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AetherRISC.CLI
{
    public class RingBufferWriter : TextWriter
    {
        private readonly int _maxLines;
        private readonly List<string> _lines;
        private readonly StringBuilder _currentLine;
        private readonly object _lock = new object();

        public override Encoding Encoding => Encoding.UTF8;

        public RingBufferWriter(int maxLines = 100)
        {
            _maxLines = maxLines;
            _lines = new List<string>(maxLines);
            _currentLine = new StringBuilder();
        }

        public override void Write(char value)
        {
            lock (_lock)
            {
                if (value == '\n')
                {
                    FlushLine();
                }
                else if (value != '\r') // Ignore CR
                {
                    _currentLine.Append(value);
                }
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (char c in value) Write(c);
        }

        public override void WriteLine(string? value)
        {
            Write(value);
            Write('\n');
        }

        private void FlushLine()
        {
            if (_lines.Count >= _maxLines)
            {
                _lines.RemoveAt(0);
            }
            _lines.Add(_currentLine.ToString());
            _currentLine.Clear();
        }

        // Fast access to data for rendering without allocating a massive string
        public List<string> Snapshot()
        {
            lock (_lock)
            {
                var result = new List<string>(_lines);
                if (_currentLine.Length > 0) result.Add(_currentLine.ToString());
                return result;
            }
        }
        
        // Needed to satisfy TextWriter contract
        public override string ToString() 
        {
            lock(_lock) return string.Join(Environment.NewLine, _lines);
        }
    }
}
