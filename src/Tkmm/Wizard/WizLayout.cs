using Tkmm.Wizard.Actions;
using Tkmm.Wizard.ViewModels;
using WizDumpConfigPage = Tkmm.Wizard.Views.WizDumpConfigPage;
using WizEmulatorSelectionPage = Tkmm.Wizard.Views.WizEmulatorSelectionPage;

namespace Tkmm.Wizard;

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
        SystemMsg.WizPage1_Title, WizEmulatorSelectionPage.Instance, [
            new WizAction(SystemMsg.WizPage1_Action_Next, 1, WizEmulatorSelectionPage.Instance.CheckSelection)
        ]);
    
    private static readonly WizPageViewModel _pageRyujinxSetup = new(
        id: 3, lastPage: _page1,
        SystemMsg.WizPageRyujinxSetup_Title, SystemMsg.WizPageRyujinxSetup_Description, [
            new WizAction(SystemMsg.WizPageRyujinxSetup_Action_Start, 0, WizActions.StartRyujinxSetup)
        ]);
    
    private static readonly WizPageViewModel _page2 = new(
        id: 2, lastPage: _page1,
        SystemMsg.WizPage2_Title, new WizDumpConfigPage(), [
            new WizAction(SystemMsg.WizPage2_Action_Verify, 0, WizActions.VerifyConfig)
        ]);
    
    private static readonly WizPageViewModel _pageFinal = new(
        id: -1, lastPage: null,
        SystemMsg.WizPageFinal_Title, SystemMsg.WizPageFinal_Description, [
            new WizAction(SystemMsg.WizPageFinal_Action_Finish, 0)
        ]);

    public static void NextPage(WizPageViewModel current, int selection)
    {
        WizPageViewModel nextPage = GetNextPage(current, selection);
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
    
    private static WizPageViewModel GetNextPage(WizPageViewModel current, int selection)
    {
        return (current.Id, selection) switch {
            (0, _) => _page1,
            (1, 0) => _pageRyujinxSetup,
            (1, 1) => _page2,
            (1, 2) => _pageFinal,
            _ => _pageFinal
        };
    }
}