using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace TaskManager.Services
{
    public static class WindowPositionManager
    {
        private static readonly List<PixelRect> _occupiedRects = new();
        private static readonly object _lock = new();

        public static int WindowWidth = 400;
        public static int WindowHeight = 400;
        public static int HorizontalSpacing = 20;
        public static int VerticalSpacing = 40;

        public static PixelRect WorkingArea { get; private set; } = new PixelRect(0, 0, 1920, 1080);

        public static void UpdateWorkingArea(Window referenceWindow)
        {
            if (referenceWindow?.Screens.Primary is Screen screen)
            {
                WorkingArea = screen.WorkingArea;
            }
        }

        public static PixelPoint RequestNextPosition()
        {
            lock (_lock)
            {
                CleanupClosedWindows();

                for (int row = 0; row < 10; row++)
                {
                    for (int col = 0; col < 10; col++)
                    {
                        int x = WorkingArea.X + col * (WindowWidth + HorizontalSpacing);
                        int y = WorkingArea.Y + row * (WindowHeight + VerticalSpacing);
                        var proposedRect = new PixelRect(x, y, WindowWidth, WindowHeight);

                        if (proposedRect.Right > WorkingArea.Right)
                            break;

                        if (proposedRect.Bottom > WorkingArea.Bottom)
                            continue;

                        if (!_occupiedRects.Any(rect => rect.Intersects(proposedRect)))
                            return new PixelPoint(x, y);
                    }
                }

                // fallback
                return new PixelPoint(WorkingArea.X + 50, WorkingArea.Y + 50);
            }
        }

        public static void RegisterWindow(Window window, PixelPoint position)
        {
            lock (_lock)
            {
                var rect = new PixelRect(position.X, position.Y, WindowWidth, WindowHeight);
                _occupiedRects.Add(rect);
                window.Closed += (s, e) =>
                {
                    lock (_lock) { _occupiedRects.Remove(rect); }
                };
            }
        }

        private static void CleanupClosedWindows() { /* события очищают, оставляем пустым */ }
    }
}