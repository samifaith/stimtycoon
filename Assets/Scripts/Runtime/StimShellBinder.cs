using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimShellBinder : IDisposable
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

        public StimShellBinder(VisualElement root, EventCallback<GeometryChangedEvent> geometryChanged)
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
            Bind(AdvanceMonth, advanceMonth);
            Bind(AdvanceYear, advanceYear);
            Bind(OpenLifeSummary, openLifeSummary);
            Bind(AddCash, openMoney);
            Bind(NavLife, showLife);
            Bind(NavEducation, showEducation);
            Bind(NavCareer, showCareer);
            Bind(NavMoney, openMoney);
            Bind(NavSocial, showSocial);
            Bind(NavGoals, showGoals);
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

        private void UnbindActions()
        {
            foreach (var binding in buttonBindings)
            {
                if (binding.Button != null) binding.Button.clicked -= binding.Callback;
            }
            buttonBindings.Clear();
        }
    }
}
