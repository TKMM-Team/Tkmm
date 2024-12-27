using Tkmm.ViewModels.Wizard;

namespace Tkmm.Models.Wizard;

public class WizLayout
{
    public static WizPageViewModel FirstPage {
        get {
            WizPageViewModel result = new(-1, null, string.Empty, string.Empty, []);
            UpdatePage(result, _page0);
            return result;
        }
    }
        
    private static readonly WizPageViewModel _page0 = new(
        id: 0, lastPage: null,
        SystemMsg.WizPage0_Title, SystemMsg.WizPage0_Description, [
            new WizAction(SystemMsg.WizPage0_Action, 0)
        ]);
    
    private static readonly WizPageViewModel _page1 = new(
        id: 1, lastPage: FirstPage,
        SystemMsg.WizPage1_Title, SystemMsg.WizPage1_Description, [
            new WizAction(SystemMsg.WizPage1_ChooseSwitchOnly, 2),
            new WizAction(SystemMsg.WizPage1_ChooseOther, 1),
            new WizAction(SystemMsg.WizPage1_ChooseRyujinx, 0),
        ]);
    
    private static readonly WizPageViewModel _page1ChoiceA = new(
        id: 1, lastPage: _page1,
        SystemMsg.WizPage1A_Title, SystemMsg.WizPage1A_Description, [
            new WizAction(SystemMsg.WizPage1A_Start, 0)
        ]);

    public static void NextPage(WizPageViewModel current, int selection)
    {
        if (GetNextPage(current, selection) is not WizPageViewModel nextPage) {
            // TODO: Set to last page
            return;
        }
        
        UpdatePage(current, nextPage);
    }

    public static void UpdatePage(WizPageViewModel current, WizPageViewModel target)
    {
        current.Id = target.Id;
        current.LastPage = target.LastPage;
        current.Title = target.Title;
        current.PageContent = target.PageContent;
        current.Actions = target.Actions;
    }
    
    private static WizPageViewModel? GetNextPage(WizPageViewModel current, int selection)
    {
        return current.Id switch {
            0 => selection switch {
                0 => _page1,
                _ => null
            },
            1 => selection switch {
                0 => _page1ChoiceA, // ryujinx
                // 0 => _page0A, // switch only
                // 1 => _page0A, // other emulator
            },
            _ => null
        };
    }
}