using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace App2.Pages
{
    /// <summary>
    /// Canvas版本画板页面 - 支持画笔、橡皮擦、颜色选择等功能
    /// </summary>
    public sealed partial class DrawingPage : Page
    {
        #region 绘图工具枚举
        public enum DrawingTool
        {
            Brush,
            Eraser
        }
        #endregion

        #region 私有字段
        private DrawingTool _currentTool = DrawingTool.Brush;
        private SolidColorBrush _currentBrush = new SolidColorBrush(Colors.Black);
        private double _brushSize = 5;
        private bool _isDrawing = false;
        private Polyline _currentStroke;

        // 撤销/重做功能
        private Stack<UIElement> _undoStack = new Stack<UIElement>();
        private Stack<UIElement> _redoStack = new Stack<UIElement>();
        #endregion

        public DrawingPage()
        {
            this.InitializeComponent();
            InitializePage();
        }

        #region 初始化
        private void InitializePage()
        {
            // 更新状态栏
            UpdateStatusText();

            // 确保Canvas可以接收pointer事件
            DrawingCanvas.Background = new SolidColorBrush(Colors.Transparent);
        }
        #endregion

        #region 工具栏事件处理
        private void BrushTool_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentTool = DrawingTool.Brush;
                if (BrushTool != null) BrushTool.IsChecked = true;
                if (EraserTool != null) EraserTool.IsChecked = false;
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BrushTool_Click错误: {ex.Message}");
            }
        }

        private void EraserTool_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentTool = DrawingTool.Eraser;
                if (EraserTool != null) EraserTool.IsChecked = true;
                if (BrushTool != null) BrushTool.IsChecked = false;
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EraserTool_Click错误: {ex.Message}");
            }
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取按钮位置来定位弹出窗口
                if (sender is AppBarButton button && ColorPickerPopup != null)
                {
                    var transform = button.TransformToVisual(this);
                    var point = transform.TransformPoint(new Point(0, 0));

                    ColorPickerPopup.HorizontalOffset = point.X;
                    ColorPickerPopup.VerticalOffset = point.Y + button.ActualHeight;
                    ColorPickerPopup.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ColorPicker_Click错误: {ex.Message}");
                // 备用方案：直接在中央显示弹出窗口
                if (ColorPickerPopup != null)
                {
                    ColorPickerPopup.HorizontalOffset = 200;
                    ColorPickerPopup.VerticalOffset = 200;
                    ColorPickerPopup.IsOpen = true;
                }
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string colorName)
                {
                    var color = GetColorFromName(colorName);
                    _currentBrush = new SolidColorBrush(color);
                    UpdateStatusText();

                    if (ColorPickerPopup != null)
                        ColorPickerPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ColorButton_Click错误: {ex.Message}");
                // 使用默认黑色作为备用
                _currentBrush = new SolidColorBrush(Colors.Black);
                UpdateStatusText();
            }
        }

        private Windows.UI.Color GetColorFromName(string colorName)
        {
            return colorName switch
            {
                "Black" => Colors.Black,
                "Red" => Colors.Red,
                "Blue" => Colors.Blue,
                "Green" => Colors.Green,
                "Yellow" => Colors.Yellow,
                "Orange" => Colors.Orange,
                "Purple" => Colors.Purple,
                "Pink" => Colors.Pink,
                "Brown" => Colors.Brown,
                "Gray" => Colors.Gray,
                "LightBlue" => Colors.LightBlue,
                "LightGreen" => Colors.LightGreen,
                _ => Colors.Black
            };
        }

        private void BrushSizeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    _brushSize = e.NewValue;
                    UpdateStatusText();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BrushSizeSlider_ValueChanged错误: {ex.Message}");
            }
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DrawingCanvas?.Children != null)
                {
                    // 保存当前状态用于撤销
                    foreach (UIElement element in DrawingCanvas.Children.ToList())
                    {
                        _undoStack.Push(element);
                    }

                    DrawingCanvas.Children.Clear();
                    _redoStack.Clear();
                    UpdateUndoRedoButtons();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearCanvas_Click错误: {ex.Message}");
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_undoStack.Count > 0 && DrawingCanvas?.Children != null)
                {
                    var element = _undoStack.Pop();
                    if (element != null && DrawingCanvas.Children.Contains(element))
                    {
                        DrawingCanvas.Children.Remove(element);
                        _redoStack.Push(element);
                    }
                    UpdateUndoRedoButtons();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Undo_Click错误: {ex.Message}");
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_redoStack.Count > 0 && DrawingCanvas?.Children != null)
                {
                    var element = _redoStack.Pop();
                    if (element != null)
                    {
                        DrawingCanvas.Children.Add(element);
                        _undoStack.Push(element);
                    }
                    UpdateUndoRedoButtons();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redo_Click错误: {ex.Message}");
            }
        }
        #endregion

        #region 绘图事件处理
        private void DrawingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var canvas = sender as Canvas;
                if (canvas == null || e?.Pointer == null) return;

                canvas.CapturePointer(e.Pointer);

                var currentPoint = e.GetCurrentPoint(canvas);
                if (currentPoint == null) return;

                var position = currentPoint.Position;
                _isDrawing = true;

                if (_currentTool == DrawingTool.Brush)
                {
                    StartNewStroke(position);
                }
                else if (_currentTool == DrawingTool.Eraser)
                {
                    EraseAtPoint(position);
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointerPressed错误: {ex.Message}");
                _isDrawing = false;
                _currentStroke = null;
            }
        }

        private void DrawingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (!_isDrawing) return;

                var canvas = sender as Canvas;
                if (canvas == null) return;

                var currentPoint = e.GetCurrentPoint(canvas);
                if (currentPoint == null) return;

                var position = currentPoint.Position;

                if (_currentTool == DrawingTool.Brush && _currentStroke != null)
                {
                    // 添加点到当前笔画
                    _currentStroke.Points.Add(position);
                }
                else if (_currentTool == DrawingTool.Eraser)
                {
                    EraseAtPoint(position);
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointerMoved错误: {ex.Message}");
                _isDrawing = false;
                _currentStroke = null;
            }
        }

        private void DrawingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var canvas = sender as Canvas;
                if (canvas == null || e?.Pointer == null) return;

                canvas.ReleasePointerCapture(e.Pointer);

                if (_isDrawing && _currentTool == DrawingTool.Brush && _currentStroke != null)
                {
                    // 将完成的笔画添加到撤销栈
                    _undoStack.Push(_currentStroke);
                    _redoStack.Clear(); // 清除重做栈
                    UpdateUndoRedoButtons();
                }

                _isDrawing = false;
                _currentStroke = null;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PointerReleased错误: {ex.Message}");
                _isDrawing = false;
                _currentStroke = null;
            }
        }

        private void DrawingCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isDrawing = false;
            _currentStroke = null;
        }
        #endregion

        #region 绘图辅助方法
        private void StartNewStroke(Point startPoint)
        {
            try
            {
                _currentStroke = new Polyline
                {
                    Stroke = new SolidColorBrush(_currentBrush.Color),
                    StrokeThickness = _brushSize,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                _currentStroke.Points.Add(startPoint);
                DrawingCanvas.Children.Add(_currentStroke);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartNewStroke错误: {ex.Message}");
                _currentStroke = null;
            }
        }

        private void EraseAtPoint(Point point)
        {
            try
            {
                var eraseRadius = Math.Max(_brushSize * 2, 10); // 橡皮擦比画笔稍大
                var elementsToRemove = new List<UIElement>();

                foreach (UIElement element in DrawingCanvas.Children)
                {
                    if (element is Polyline polyline && IsPolylineIntersecting(polyline, point, eraseRadius))
                    {
                        elementsToRemove.Add(element);
                    }
                }

                // 移除相交的元素
                foreach (var element in elementsToRemove)
                {
                    DrawingCanvas.Children.Remove(element);
                    _undoStack.Push(element); // 添加到撤销栈以支持撤销擦除
                }

                if (elementsToRemove.Count > 0)
                {
                    _redoStack.Clear();
                    UpdateUndoRedoButtons();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EraseAtPoint错误: {ex.Message}");
            }
        }

        private bool IsPolylineIntersecting(Polyline polyline, Point point, double radius)
        {
            try
            {
                if (polyline?.Points == null || polyline.Points.Count < 2)
                    return false;

                // 简单的碰撞检测：检查点是否在多边形线段附近
                for (int i = 0; i < polyline.Points.Count - 1; i++)
                {
                    var p1 = polyline.Points[i];
                    var p2 = polyline.Points[i + 1];

                    if (DistanceFromPointToLineSegment(point, p1, p2) <= radius)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsPolylineIntersecting错误: {ex.Message}");
                return false;
            }
        }

        private double DistanceFromPointToLineSegment(Point point, Point lineStart, Point lineEnd)
        {
            try
            {
                var A = point.X - lineStart.X;
                var B = point.Y - lineStart.Y;
                var C = lineEnd.X - lineStart.X;
                var D = lineEnd.Y - lineStart.Y;

                var dot = A * C + B * D;
                var lenSq = C * C + D * D;

                if (lenSq == 0)
                    return Math.Sqrt(A * A + B * B);

                var param = dot / lenSq;

                double xx, yy;

                if (param < 0)
                {
                    xx = lineStart.X;
                    yy = lineStart.Y;
                }
                else if (param > 1)
                {
                    xx = lineEnd.X;
                    yy = lineEnd.Y;
                }
                else
                {
                    xx = lineStart.X + param * C;
                    yy = lineStart.Y + param * D;
                }

                var dx = point.X - xx;
                var dy = point.Y - yy;
                return Math.Sqrt(dx * dx + dy * dy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DistanceFromPointToLineSegment错误: {ex.Message}");
                return double.MaxValue;
            }
        }
        #endregion

        #region UI更新方法
        private void UpdateStatusText()
        {
            try
            {
                if (StatusText != null)
                {
                    var toolName = _currentTool == DrawingTool.Brush ? "画笔工具" : "橡皮擦工具";
                    var colorName = GetColorName(_currentBrush?.Color ?? Colors.Black);
                    StatusText.Text = $"{toolName} | 大小: {_brushSize:F0} | 颜色: {colorName}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatusText错误: {ex.Message}");
            }
        }

        private string GetColorName(Windows.UI.Color color)
        {
            if (color == Colors.Black) return "黑色";
            if (color == Colors.Red) return "红色";
            if (color == Colors.Blue) return "蓝色";
            if (color == Colors.Green) return "绿色";
            if (color == Colors.Yellow) return "黄色";
            if (color == Colors.Orange) return "橙色";
            if (color == Colors.Purple) return "紫色";
            if (color == Colors.Pink) return "粉色";
            if (color == Colors.Brown) return "棕色";
            if (color == Colors.Gray) return "灰色";
            if (color == Colors.LightBlue) return "浅蓝色";
            if (color == Colors.LightGreen) return "浅绿色";
            return "自定义";
        }

        private void UpdateUndoRedoButtons()
        {
            try
            {
                if (UndoButton != null)
                    UndoButton.IsEnabled = _undoStack.Count > 0;

                if (RedoButton != null)
                    RedoButton.IsEnabled = _redoStack.Count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUndoRedoButtons错误: {ex.Message}");
            }
        }
        #endregion
    }
}