using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("StimTycoon.Tests")]
[assembly: InternalsVisibleTo("StimTycoon.PlayModeTests")]

namespace StimTycoon.Runtime
{
    internal enum ShellModal
    {
        None,
        Event,
        StudySession,
        NewLife,
        FinalLifeSummary
    }

    internal sealed class ShellBinder : IDisposable
    {
        private readonly VisualElement root;
        private readonly EventCallback<GeometryChangedEvent> geometryChanged;
        private readonly List<ButtonBinding> buttonBindings = new List<ButtonBinding>();

        private readonly struct ButtonBinding
        {
            public readonly Button Button;
            public readonly Action Callback;

            public ButtonBinding(Button button, Action callback)
            {
                Button = button;
                Callback = callback;
            }
        }

        public ShellBinder(VisualElement root, EventCallback<GeometryChangedEvent> geometryChanged)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
            this.geometryChanged = geometryChanged;

            Screen = root.Q<VisualElement>("screen");
            CashValue = root.Q<Label>("cash-value");
            LifeSummary = root.Q<Label>("life-summary");
            CalendarSummary = root.Q<Label>("calendar-summary");
            HeaderNetWorthValue = root.Q<Label>("header-net-worth-value");
            AvatarGlyph = root.Q<Label>("avatar-glyph");
            CareerProgressValue = root.Q<Label>("career-progress-value");
            CareerProgressFill = root.Q<VisualElement>("career-progress-fill");
            OpenLifeSummary = root.Q<Button>("open-life-summary");
            AddCash = root.Q<Button>("add-cash");
            LifeScroll = root.Q<ScrollView>("life-scroll");
            LifeSummaryView = root.Q<ScrollView>("life-summary-view");
            EducationView = root.Q<ScrollView>("education-view");
            CareerView = root.Q<ScrollView>("career-view");
            MoneyView = root.Q<ScrollView>("money-view");
            SocialView = root.Q<ScrollView>("social-view");
            GoalsView = root.Q<ScrollView>("goals-view");
            TimeDock = root.Q<VisualElement>("time-dock");
            AdvanceMonth = root.Q<Button>("advance-month");
            AdvanceYear = root.Q<Button>("advance-year");
            NavLife = root.Q<Button>("nav-life");
            NavEducation = root.Q<Button>("nav-education");
            NavCareer = root.Q<Button>("nav-career");
            NavMoney = root.Q<Button>("nav-money");
            NavSocial = root.Q<Button>("nav-social");
            NavGoals = root.Q<Button>("nav-goals");
            EventSheet = root.Q<VisualElement>("event-sheet");
            StudySessionSheet = root.Q<VisualElement>("study-session-sheet");
            NewLifeSetup = root.Q<VisualElement>("new-life-setup");
            FinalLifeSummary = root.Q<VisualElement>("final-life-summary");

            if (geometryChanged != null) root.RegisterCallback(geometryChanged);
        }

        public VisualElement Screen { get; }
        public Label CashValue { get; }
        public Label LifeSummary { get; }
        public Label CalendarSummary { get; }
        public Label HeaderNetWorthValue { get; }
        public Label AvatarGlyph { get; }
        public Label CareerProgressValue { get; }
        public VisualElement CareerProgressFill { get; }
        public Button OpenLifeSummary { get; }
        public Button AddCash { get; }
        public ScrollView LifeScroll { get; }
        public ScrollView LifeSummaryView { get; }
        public ScrollView EducationView { get; }
        public ScrollView CareerView { get; }
        public ScrollView MoneyView { get; }
        public ScrollView SocialView { get; }
        public ScrollView GoalsView { get; }
        public VisualElement TimeDock { get; }
        public Button AdvanceMonth { get; }
        public Button AdvanceYear { get; }
        public Button NavLife { get; }
        public Button NavEducation { get; }
        public Button NavCareer { get; }
        public Button NavMoney { get; }
        public Button NavSocial { get; }
        public Button NavGoals { get; }
        public VisualElement EventSheet { get; }
        public VisualElement StudySessionSheet { get; }
        public VisualElement NewLifeSetup { get; }
        public VisualElement FinalLifeSummary { get; }
        public ShellModal ActiveModal { get; private set; }
        public Destination ActiveDestination { get; private set; } = Destination.Life;
        public Destination ModalReturnDestination { get; private set; } = Destination.Life;
        public string ModalReturnTabId { get; private set; } = string.Empty;
        public string ModalReturnEntityId { get; private set; } = string.Empty;

        public bool HasBlockingModal => ActiveModal != ShellModal.None;

        public bool TryRunAction(Action action, ShellModal owningModal = ShellModal.None)
        {
            if (action == null) return false;
            if (HasBlockingModal && ActiveModal != owningModal) return false;
            action();
            return true;
        }

        public void CaptureModalReturnContext(string tabId, string entityId)
        {
            if (HasBlockingModal) return;
            ModalReturnDestination = ActiveDestination;
            ModalReturnTabId = tabId ?? string.Empty;
            ModalReturnEntityId = entityId ?? string.Empty;
        }

        public void OpenModal(ShellModal modal)
        {
            if (modal == ShellModal.None)
            {
                CloseAllModals();
                return;
            }

            if (!HasBlockingModal) ModalReturnDestination = ActiveDestination;
            foreach (var entry in GetModals())
                entry.Element?.EnableInClassList("hidden", entry.Modal != modal);
            ActiveModal = modal;
        }

        public void CloseModal(ShellModal modal)
        {
            GetModal(modal)?.AddToClassList("hidden");
            if (ActiveModal == modal) ActiveModal = ShellModal.None;
        }

        public void CloseAllModals()
        {
            foreach (var entry in GetModals()) entry.Element?.AddToClassList("hidden");
            ActiveModal = ShellModal.None;
        }

        public ScrollView RestoreModalReturnDestination()
        {
            CloseAllModals();
            return RenderDestination(ModalReturnDestination);
        }

        public ScrollView RenderDestination(Destination destination)
        {
            ActiveDestination = destination;
            var selectedView = GetDestinationView(destination);
            var selectedButton = GetDestinationButton(destination);

            foreach (var view in GetDestinationViews(includeLifeSummary: true))
                view?.EnableInClassList("hidden", view != selectedView);
            foreach (var button in GetDestinationButtons())
                button?.EnableInClassList("active", button == selectedButton);

            TimeDock?.EnableInClassList("hidden", destination != Destination.Life);
            return selectedView;
        }

        public void RenderLifeSummary()
        {
            foreach (var view in GetDestinationViews(includeLifeSummary: false))
                view?.AddToClassList("hidden");
            LifeSummaryView?.RemoveFromClassList("hidden");
            TimeDock?.AddToClassList("hidden");
        }

        public bool TryRunShellAction(Action callback)
        {
            return TryRunAction(callback);
        }

        public void BindActions(
            Action advanceMonth,
            Action advanceYear,
            Action openLifeSummary,
            Action openMoney,
            Action showLife,
            Action showEducation,
            Action showCareer,
            Action showSocial,
            Action showGoals)
        {
            UnbindActions();
            BindShellAction(AdvanceMonth, advanceMonth);
            BindShellAction(AdvanceYear, advanceYear);
            BindShellAction(OpenLifeSummary, openLifeSummary);
            BindShellAction(AddCash, openMoney);
            BindShellAction(NavLife, showLife);
            BindShellAction(NavEducation, showEducation);
            BindShellAction(NavCareer, showCareer);
            BindShellAction(NavMoney, openMoney);
            BindShellAction(NavSocial, showSocial);
            BindShellAction(NavGoals, showGoals);
        }

        public void Dispose()
        {
            UnbindActions();
            if (geometryChanged != null) root.UnregisterCallback(geometryChanged);
        }

        private void Bind(Button button, Action callback)
        {
            if (button == null || callback == null) return;
            button.clicked -= callback;
            button.clicked += callback;
            buttonBindings.Add(new ButtonBinding(button, callback));
        }

        private void BindShellAction(Button button, Action callback)
        {
            if (button == null || callback == null) return;
            Action guardedCallback = () =>
            {
                TryRunShellAction(callback);
            };
            Bind(button, guardedCallback);
        }

        private void UnbindActions()
        {
            foreach (var binding in buttonBindings)
            {
                if (binding.Button != null) binding.Button.clicked -= binding.Callback;
            }
            buttonBindings.Clear();
        }

        private VisualElement GetModal(ShellModal modal)
        {
            switch (modal)
            {
                case ShellModal.Event: return EventSheet;
                case ShellModal.StudySession: return StudySessionSheet;
                case ShellModal.NewLife: return NewLifeSetup;
                case ShellModal.FinalLifeSummary: return FinalLifeSummary;
                default: return null;
            }
        }

        private IEnumerable<(ShellModal Modal, VisualElement Element)> GetModals()
        {
            yield return (ShellModal.Event, EventSheet);
            yield return (ShellModal.StudySession, StudySessionSheet);
            yield return (ShellModal.NewLife, NewLifeSetup);
            yield return (ShellModal.FinalLifeSummary, FinalLifeSummary);
        }

        private ScrollView GetDestinationView(Destination destination)
        {
            switch (destination)
            {
                case Destination.Study: return EducationView;
                case Destination.Work: return CareerView;
                case Destination.Bank: return MoneyView;
                case Destination.Social: return SocialView;
                case Destination.Goals: return GoalsView;
                default: return LifeScroll;
            }
        }

        private Button GetDestinationButton(Destination destination)
        {
            switch (destination)
            {
                case Destination.Study: return NavEducation;
                case Destination.Work: return NavCareer;
                case Destination.Bank: return NavMoney;
                case Destination.Social: return NavSocial;
                case Destination.Goals: return NavGoals;
                default: return NavLife;
            }
        }

        private IEnumerable<VisualElement> GetDestinationViews(bool includeLifeSummary)
        {
            yield return LifeScroll;
            yield return EducationView;
            yield return CareerView;
            yield return MoneyView;
            yield return SocialView;
            yield return GoalsView;
            if (includeLifeSummary) yield return LifeSummaryView;
        }

        private IEnumerable<Button> GetDestinationButtons()
        {
            yield return NavLife;
            yield return NavEducation;
            yield return NavCareer;
            yield return NavMoney;
            yield return NavSocial;
            yield return NavGoals;
        }
    }
}
