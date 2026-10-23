using Tkmm.Core;

namespace Tkmm.Views.Common;

public partial class MergingModal : OverlayCard
{
    public MergingModal()
    {
        InitializeComponent();
    }
    
    public static void ShowModal(CancellationToken cancellationToken)
    {
        if (!Config.Shared.ShowTriviaPopup) {
            return;
        }

        OverlayModal.Show(new MergingModal(), cancellationToken);
    }
}