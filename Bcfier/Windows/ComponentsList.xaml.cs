using System.Windows;
using Bcfier.Bcf.Model;


namespace Bcfier.Windows
{
  /// <summary>
  /// Interaction logic for Components.xaml
  /// </summary>
  public partial class ComponentsList : Window
  {
    //disabled for now
    public ComponentsList(BcfComponentsData components)
    {
      InitializeComponent();
      //componentsList.ItemsSource = components.Selection;

    }
  }
}
