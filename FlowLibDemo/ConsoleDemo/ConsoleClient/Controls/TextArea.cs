
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;

namespace ConsoleDemo.ConsoleClient.Controls
{
    public class TextArea : Rectangle, IFocusable
    {
        protected List<string> text = new List<string>();
        protected int rowTop = 0;
        protected bool locked = true;

        public int TopRowIndex
        {
            get { return rowTop; }
        }

        public bool ReadOnly
        {
            get { return locked; }
            set { locked = value; }
        }

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder(text.Capacity);
                foreach (string str in text)
                {
                    sb.AppendLine(str);
                }
                return sb.ToString();
            }
            set
            {
                //if (!locked)
                //{
                    string tmp = value.Replace("\r", "");
                    text = new List<string>(tmp.Split('\n'));
                    if (!hidden)
                        Show();
                //}
            }
        }

        public TextArea(int x, int y, int width, int height)
            : this(x, y, width, height, height)
        {

        }
        public TextArea(int x, int y, int width, int height, int maxRows)
            : base(x, y, width, height)
        {
            text.Capacity = maxRows;
        }

        public virtual int Focus()
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.CursorLeft = X;
            Console.CursorTop = Y;
            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;

            ConsoleKeyInfo key;
            int value = -1;
            do
            {
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    #region Navigate
                    case ConsoleKey.UpArrow:
                        if (Console.CursorTop > Y)
                            Console.CursorTop--;
                        else if (rowTop > 0)
                        {
                            rowTop--;
                            Show();
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (Console.CursorTop < (Y + height -1))
                            Console.CursorTop++;
                        else if ((rowTop + height) < text.Count)
                        {
                            rowTop++;
                            Show();
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (Console.CursorLeft > X)
                            Console.CursorLeft--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (Console.CursorLeft < (X + width - 2))
                            Console.CursorLeft++;
                        break;
                    case ConsoleKey.PageUp:
                        if (Console.CursorTop > Y)
                            Console.CursorTop = Y;
                        else if (rowTop > 0)
                        {
                            rowTop -= height;
                            if (rowTop < 0)
                                rowTop = 0;
                            Show();
                        }
                        break;
                    case ConsoleKey.PageDown:
                        if (Console.CursorTop < (Y + height - 1))
                            Console.CursorTop = Y + height - 1;
                        else if ((rowTop + height) < text.Count)
                        {
                            rowTop += height;
                            if ((rowTop + height) > text.Count)
                                rowTop = text.Count - height;
                            Show();
                        }
                        break;
                    #endregion
                    #region Add/Edit stuff
                    default:
                        if (!locked)
                        {
                            // TODO: Right now we cant add lines
                            int index = 0;
                            if ((index = Console.CursorTop - Y) > text.Count)
                            {
                                while ((Console.CursorTop - Y) > text.Count)
                                {
                                    text.Insert(text.Count, "");
                                }
                            }
                            string str = text[index];
                            int indexL = Console.CursorLeft - X;
                            while (indexL >= str.Length)
                            {
                                str += " ";
                            }
                            str = str.Remove(indexL, 1);
                            str = str.Insert(indexL, key.KeyChar.ToString());
                            text[index] = str;
                            Console.Write(key.KeyChar);
                        }
                        break;
                    case ConsoleKey.Backspace:
                        if (!locked)
                        {
                            // TODO: Right now we cant add lines
                            int index = 0;
                            if ((index = Console.CursorTop - Y) > text.Count)
                            {
                                while ((Console.CursorTop - Y) > text.Count)
                                {
                                    text.Insert(text.Count, "");
                                }
                            }
                            string str = text[index];
                            int indexL = Console.CursorLeft - X;
                            while (indexL >= str.Length)
                            {
                                str += " ";
                            }

                            if (Console.CursorLeft > X)
                            {
                                Console.CursorVisible = false;
                                str = str.Remove(Console.CursorLeft - (X + 1), 1);
                                text[index] = str;
                                Console.CursorLeft--;
                                Console.Write(" ");
                                Console.CursorLeft--;
                                Console.CursorVisible = true;
                            }

                        }
                        break;
                    #endregion
                    case ConsoleKey.Tab:
                        value = 1;
                        break;
                    case ConsoleKey.Enter:
                        value = 0;
                        break;
                }
            } while (value == -1);
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            return value;
        }

        public override void Show()
        {
            base.Show();

            int cX = Console.CursorLeft;
            int cY = Console.CursorTop;
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;

            // Do content exist above?
            if (rowTop > 0)
            {
                Console.CursorLeft = posX + width - 1;
                Console.CursorTop = posY;
                Console.Write("▲");
            }
            // Do content exist below?
            if ((rowTop + height) < text.Count)
            {
                Console.CursorLeft = posX + width - 1;
                Console.CursorTop = posY + height - 1;
                Console.Write("▼");
            }

            #region Text
            for (int y = rowTop; y < text.Count && y < (height + rowTop); y++)
            {
                string str = text[y];
                if (str.Length >= width)
                    str = str.Substring(0, width - 1);
                Console.CursorLeft = X;
                Console.CursorTop = (y - rowTop) + Y;
                Console.Write(str);
            }
            #endregion

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.CursorLeft = cX;
            Console.CursorTop = cY;
            Console.CursorVisible = true;
        }
    }
}
