using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using Sudoku.Enumerations;

namespace Sudoku.Extensions;

   public static class VisualExtensions
   {
      public static async Task<bool> ShowMessageBox(this Visual visual, string message, Action<Window>? preShowAction = null)
      {
         Label label = new() { Content = message };
         return await visual.ShowDialog(label, DialogButtonType.Ok, preShowAction);
      }

      public static async Task<string?> ShowInputBox(this Visual visual, string prompt, string defaultValue = "", Action<Window>? preShowAction = null)
      {
         Label label = new() { Content = prompt };
         TextBox textBox = new() { Text = defaultValue, };
         StackPanel panel = new() { Orientation = Orientation.Vertical };
         panel.Children.Add(label);
         panel.Children.Add(textBox);

         bool result = await visual.ShowDialog(panel, DialogButtonType.Ok | DialogButtonType.Cancel, preShowAction);
         return result ? textBox.Text : null;
      }

      public static async Task<bool> ShowDialog(this Visual visual, Control content, DialogButtonType buttonTypes, Action<Window>? preShowAction = null)
      {
         Window? window = TopLevel.GetTopLevel(visual) as Window;
         if (window == null)
         {
            throw new Exception($"Unable to locate top level window for {visual}");
         }

         Grid mainPanel = new();
         mainPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
         mainPanel.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
         mainPanel.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
         mainPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

         Window dialog = new()
         {
            Width = double.NaN,
            Height = double.NaN,
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = mainPanel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
         };

         content.Margin = new Thickness(10);
         Grid.SetRow(content, 0);
         Grid.SetColumn(content, 0);
         Grid.SetColumnSpan(content, 2);
         mainPanel.Children.Add(content);

         StackPanel buttonPanel = new() { Orientation = Orientation.Horizontal };
         Grid.SetRow(buttonPanel, 1);
         Grid.SetColumn(buttonPanel, 1);
         mainPanel.Children.Add(buttonPanel);

         foreach (DialogButtonType buttonType in Enum.GetValues(typeof(DialogButtonType)))
         {
            if (buttonType == DialogButtonType.None) { continue; }
            if (!buttonTypes.HasFlag(buttonType)) { continue; }

            buttonPanel.Children.Add(
               new Button()
               {
                  Content = buttonType.ToString(), 
                  IsDefault = buttonType == DialogButtonType.Ok, 
                  IsCancel = buttonType == DialogButtonType.Cancel,
                  Command = ReactiveCommand.Create(() => dialog.Close((int)buttonType)),
                  Margin = new Thickness(0, 0, 5, 5),
               }
            );
         }

         try
         {
            preShowAction?.Invoke(dialog);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
         
         int result = await dialog.ShowDialog<int>(window);
         return (DialogButtonType)result == DialogButtonType.Ok;
      }
   }
