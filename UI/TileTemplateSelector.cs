using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App2.Models;   // 引用 TileKind / FileKind 枚举
// ★新增 文件：UI/TileTemplateSelector.cs
using App2.Pages;                // 引用 TileItem 所在命名空间
             
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App2.UI
{
    public sealed class TileTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageTemplate { get; set; }
        public DataTemplate? NoteTemplate { get; set; }
        public DataTemplate? FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TileItem t)
            {
                return t.Kind switch
                {
                    TileKind.Image => ImageTemplate ?? base.SelectTemplateCore(item, container),
                    TileKind.Note => NoteTemplate ?? base.SelectTemplateCore(item, container),
                    TileKind.File => FileTemplate ?? base.SelectTemplateCore(item, container),
                    _ => base.SelectTemplateCore(item, container)
                };
            }
            return base.SelectTemplateCore(item, container);
        }

        protected override DataTemplate SelectTemplateCore(object item)
            => SelectTemplateCore(item, null!);
    }
}
