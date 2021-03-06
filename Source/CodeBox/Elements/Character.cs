﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CodeBox.Core;
using CodeBox.Core.Utilities;
using CodeBox.CodeLexer;

namespace CodeBox.Core.Elements
{
	public class Character : Grid
	{
		public enum CharState { Normal, UnderlinedBlue, UnderlinedRed }

		public const double CHAR_WIDTH = 8.0;
		public const double CHAR_HEIGHT = 15.0;

		public int Line { get; set; }
		public int Position { get; set; }
		public char Char
		{
			get
			{
				return text.Text[0];
			}

			set
			{
				text.Text = value.ToString();
			}
		}
		public bool IsTab
		{
			get
			{
				return text.Text == "\t";
			}
		}
		public Token ParentToken { get; set; }

		#region char type (keyword, symbol, etc)
		private TokenType _charType = TokenType.EOF;		
		public TokenType CharType
		{
			get
			{
				return _charType;
			}

			set
			{
				_charType = value;
				UpdateState();
			}
		}
		#endregion
		
		#region UIE children
		private Border border;
		private TextBlock text;
		private Path path;
		#endregion

		public Character(char c, int line, int position)
		{
			#region set defalt properties
			Height = CHAR_HEIGHT;
			Width = (c == '\t') ? CHAR_WIDTH * 4 : CHAR_WIDTH;
			#endregion

			#region initialize children
			// border
			border = new Border();

			// text
			text = new TextBlock();
			text.FontFamily = new FontFamily("Courier New");
			text.FontSize = 13;
			text.Foreground = new SolidColorBrush(Colors.Black);
			text.Text = c.ToString();

			// path
			path = new Path();
			var pg = new PathGeometry();
			var pfc = new PathFigureCollection();
			var pf = new PathFigure();
			pf.StartPoint = new Point(0, 12);
			pf.Segments = new PathSegmentCollection();
			pf.Segments.Add(new LineSegment() { Point = new Point(2, 14)});
			pf.Segments.Add(new LineSegment() { Point = new Point(4, 12)});
			pf.Segments.Add(new LineSegment() { Point = new Point(6, 14)});
			pf.Segments.Add(new LineSegment() { Point = new Point(8, 12)});
			pfc.Add(pf);
			pg.Figures = pfc;
			path.Data = pg;
			path.Stroke = new SolidColorBrush(Colors.Red);
			path.StrokeThickness = 0.5;
			path.Visibility = Visibility.Collapsed;
			#endregion
			
			#region add children
			border.Child = text;
			Children.Add(border);
			Children.Add(path);
			#endregion

			Line = line;
			Position = position;

			DoubleClickHelper.Attach(this, OnDoubleClick);
		}

		private void OnDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (ParentToken == null)
				return;
			
			var chars = Controller.Instance.TokenChars.GetTokenCharsByToken(ParentToken);

			Controller.Instance.Paper.HighlightFrom(chars.FirstCharacter.Line, chars.FirstCharacter.Position);
			Controller.Instance.Paper.HighlightUpto(chars.FirstCharacter.Line, chars.FirstCharacter.Position + chars.Characters.Count);

			e.Handled = true;
			Controller.Instance.Paper.UpdateCaret(chars.Token.Line - 1, chars.Token.Position + chars.Characters.Count - 1);
		}

		private void UpdateState()
		{
			switch (_charType)
			{
				case TokenType.IDENTIFIER:
					text.Foreground = Configuration.SyntaxHighlighter.IdentifierColor;
					break;

				case TokenType.STRING:
					text.Foreground = Configuration.SyntaxHighlighter.StringColor;
					break;

				case TokenType.NUMBER:
					text.Foreground = Configuration.SyntaxHighlighter.NumberColor;
					break;

				case TokenType.KEYWORD:
					text.Foreground = Configuration.SyntaxHighlighter.KeywordColor;
					break;

				case TokenType.OPERATOR:
					text.Foreground = Configuration.SyntaxHighlighter.OperatorColor;
					break;

				case TokenType.SYMBOL:
					text.Foreground = Configuration.SyntaxHighlighter.SymbolColor;
					break;

				case TokenType.COMMENT:
					text.Foreground = Configuration.SyntaxHighlighter.CommentColor;
					break;
			}
		}

		public void SetState(CharState state)
		{
			border.BorderThickness = new Thickness(0);
			path.Visibility = Visibility.Collapsed;

			switch (state)
			{
				case CharState.UnderlinedBlue:
					border.BorderBrush = new SolidColorBrush(Colors.Blue);
					border.BorderThickness = new Thickness(0, 0, 0, 2.5);
					break;

				case CharState.UnderlinedRed:
					path.Visibility = Visibility.Visible;
					break;
			}
		}

		public void HighlightBlue()
		{
			text.Foreground = new SolidColorBrush(Colors.White);
			border.Background = new SolidColorBrush(Color.FromArgb(255, 55, 126, 232));
		}

		public void HighlightYellow()
		{
			border.Background = new SolidColorBrush(Color.FromArgb(255, 255, 241, 50));
		}

		public void Unhighlight()
		{
			UpdateState();
			border.Background = new SolidColorBrush(Colors.Transparent);
		}
	}

	public static class ListOfCharacterExtension
	{
		public static string GetText(this List<Character> chars)
		{
			StringBuilder result = new StringBuilder();

			foreach (var c in chars)
				result.Append(c.Char);

			return result.ToString();
		}
	}
}