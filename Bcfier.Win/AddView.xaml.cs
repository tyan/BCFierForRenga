using Bcfier.Bcf.Bcf2;
using System.Windows;

namespace Bcfier.Win
{
  /// <summary>
  /// Interaction logic for AddView.xaml
  /// </summary>
  public partial class AddView : Window
  {
    public AddView(Markup issue, string bcfTempFolder)
    {
      this.InitializeComponent();
      AddViewControl.Issue = issue;
      AddViewControl.TempFolder = bcfTempFolder;
    }
  }
}
