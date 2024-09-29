using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using PolyECS;
using PolyECS.Systems;

namespace Verse;

public partial class SpawnGuiTest : AutoSystem
{
    [AutoRunMethod]
    public void Run(Desktop desktop)
    {
        
        var grid = new Grid
        {
            RowSpacing = 8,
            ColumnSpacing = 8
        };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var helloWorld = new Label
        {
            Id = "label",
            Text = "Hello, World!"
        };
        grid.Widgets.Add(helloWorld);

// ComboBox
        var combo = new ComboBox();
        Grid.SetColumn(combo, 1);
        Grid.SetRow(combo, 0);

        combo.Items.Add(new ListItem("Red", Color.Red));
        combo.Items.Add(new ListItem("Green", Color.Green));
        combo.Items.Add(new ListItem("Blue", Color.Blue));
        grid.Widgets.Add(combo);

// Button
        var button = new Button
        {
            Content = new Label
            {
                Text = "Show"
            }
        };
        Grid.SetColumn(button, 0);
        Grid.SetRow(button, 1);

        button.Click += (s, a) =>
        {
            var messageBox = Dialog.CreateMessageBox("Message", "Some message!");
            messageBox.ShowModal(desktop);
        };

        grid.Widgets.Add(button);

// Spin button
        var spinButton = new SpinButton
        {
            Width = 100,
            Nullable = true
        };
        Grid.SetColumn(spinButton, 1);
        Grid.SetRow(spinButton, 1);

        grid.Widgets.Add(spinButton);
        desktop.Root = grid;
    }
}
