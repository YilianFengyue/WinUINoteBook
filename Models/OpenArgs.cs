using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2.Models
{
    public sealed class OpenArgs
    {
        public TileKind Kind { get; init; }
        public FileKind? FileKind { get; init; }
        public string? LocalPath { get; init; }
        public int? Page { get; init; }      // PDF 页码
        public string? Anchor { get; init; } // Markdown 锚点（后续用）
        public string? Slide { get; init; }  // PPT 目标（后续用）
        public bool ReadOnly { get; init; } = false;
    }
}
