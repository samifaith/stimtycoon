using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class BankBinder
    {
        private readonly Label savingsBalanceValue;
        private readonly Label savingsAvailableValue;
        private readonly VisualElement savingsAmountInput;
        private readonly VisualElement moneyTransactionHistory;
        private readonly VisualElement moneyAccountsList;
        private readonly Label cashFlowGross;
        private readonly Label cashFlowTaxes;
        private readonly Label cashFlowExpenses;
        private readonly Label cashFlowCreditInterest;
        private readonly Label cashFlowSavingsInterest;
        private readonly Label cashFlowNet;
        private readonly Label savingsProjection;
        private readonly Label creditBalanceValue;
        private readonly Label creditDetailValue;
        private readonly Label availableCreditValue;
        private readonly VisualElement creditRepaymentInput;
        private readonly Label indexFundValue;
        private readonly Label indexFundContributions;
        private readonly Label indexFundPerformance;
        private readonly Label indexInvestmentRequirement;
        private readonly VisualElement indexInvestmentInput;
        private readonly VisualElement bankPanelSavings;
        private readonly VisualElement bankPanelCredit;
        private readonly VisualElement bankPanelInvesting;
        private readonly VisualElement bankPanelProperty;
        private readonly Label propertyPortfolioSummary;
        private readonly Label propertyCashFlow;
        private readonly VisualElement propertyActions;
        private readonly Label propertyFeedback;
        private readonly Label bankContextTip;

        public BankBinder(VisualElement root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            savingsBalanceValue = root.Q<Label>("savings-balance-value");
            savingsAvailableValue = root.Q<Label>("savings-available-value");
            SavingsDepositMode = root.Q<Button>("savings-deposit-mode");
            SavingsWithdrawMode = root.Q<Button>("savings-withdraw-mode");
            savingsAmountInput = root.Q<VisualElement>("savings-amount-input");
            moneyTransactionHistory = root.Q<VisualElement>("money-transaction-history");
            moneyAccountsList = root.Q<VisualElement>("money-accounts-list");
            cashFlowGross = root.Q<Label>("cash-flow-gross");
            cashFlowTaxes = root.Q<Label>("cash-flow-taxes");
            cashFlowExpenses = root.Q<Label>("cash-flow-expenses");
            cashFlowCreditInterest = root.Q<Label>("cash-flow-credit-interest");
            cashFlowSavingsInterest = root.Q<Label>("cash-flow-savings-interest");
            cashFlowNet = root.Q<Label>("cash-flow-net");
            savingsProjection = root.Q<Label>("savings-projection");
            creditBalanceValue = root.Q<Label>("credit-balance-value");
            creditDetailValue = root.Q<Label>("credit-detail-value");
            availableCreditValue = root.Q<Label>("available-credit-value");
            creditRepaymentInput = root.Q<VisualElement>("credit-repayment-input");
            indexFundValue = root.Q<Label>("index-fund-value");
            indexFundContributions = root.Q<Label>("index-fund-contributions");
            indexFundPerformance = root.Q<Label>("index-fund-performance");
            indexInvestmentRequirement = root.Q<Label>("index-investment-requirement");
            indexInvestmentInput = root.Q<VisualElement>("index-investment-input");
            BankTabSavings = root.Q<Button>("bank-tab-savings");
            BankTabCredit = root.Q<Button>("bank-tab-credit");
            BankTabInvesting = root.Q<Button>("bank-tab-investing");
            BankTabProperty = root.Q<Button>("bank-tab-property");
            bankPanelSavings = root.Q<VisualElement>("bank-panel-savings");
            bankPanelCredit = root.Q<VisualElement>("bank-panel-credit");
            bankPanelInvesting = root.Q<VisualElement>("bank-panel-investing");
            bankPanelProperty = root.Q<VisualElement>("bank-panel-property");
            propertyPortfolioSummary = root.Q<Label>("property-portfolio-summary");
            propertyCashFlow = root.Q<Label>("property-cash-flow");
            propertyActions = root.Q<VisualElement>("property-actions");
            propertyFeedback = root.Q<Label>("property-feedback");
            bankContextTip = root.Q<Label>("bank-context-tip");
        }

        public Button SavingsDepositMode { get; }
        public Button SavingsWithdrawMode { get; }
        public Button BankTabSavings { get; }
        public Button BankTabCredit { get; }
        public Button BankTabInvesting { get; }
        public Button BankTabProperty { get; }

        public bool IsValid => savingsBalanceValue != null && savingsAvailableValue != null &&
            SavingsDepositMode != null && SavingsWithdrawMode != null && savingsAmountInput != null &&
            moneyTransactionHistory != null && moneyAccountsList != null && cashFlowGross != null &&
            cashFlowTaxes != null && cashFlowExpenses != null && cashFlowCreditInterest != null &&
            cashFlowSavingsInterest != null && cashFlowNet != null && savingsProjection != null &&
            creditBalanceValue != null && creditDetailValue != null && availableCreditValue != null &&
            creditRepaymentInput != null && indexFundValue != null && indexFundContributions != null &&
            indexFundPerformance != null && indexInvestmentRequirement != null && indexInvestmentInput != null &&
            BankTabSavings != null && BankTabCredit != null && BankTabInvesting != null && BankTabProperty != null &&
            bankPanelSavings != null && bankPanelCredit != null && bankPanelInvesting != null && bankPanelProperty != null &&
            propertyPortfolioSummary != null && propertyCashFlow != null && propertyActions != null && propertyFeedback != null &&
            bankContextTip != null;

        public BankTab Render(GameState state, SavingsTransferType transferType, BankTab selectedTab,
            Func<long, string> formatMoney, Func<long, string> formatSignedMoney,
            Action<long> transfer, Action clearTransferFeedback, Action<long> repayCredit, Action<long> invest,
            Action<PropertyActionType, string> manageProperty)
        {
            var adult = state.character.age >= 18;
            var finances = state.finances;
            RenderFinancialState(finances, formatMoney);
            savingsBalanceValue.text = formatMoney(finances.savingsMinorUnits);
            RenderAccounts(finances, adult, formatMoney);
            indexInvestmentInput.parent?.EnableInClassList("hidden", !adult);
            var available = transferType == SavingsTransferType.Deposit ? finances.cashMinorUnits : finances.savingsMinorUnits;
            savingsAvailableValue.text = transferType == SavingsTransferType.Deposit
                ? $"Available cash: {formatMoney(available)}" : $"Available savings: {formatMoney(available)}";
            SavingsDepositMode.EnableInClassList("active", transferType == SavingsTransferType.Deposit);
            SavingsWithdrawMode.EnableInClassList("active", transferType == SavingsTransferType.Withdrawal);
            savingsAmountInput.Clear();
            var depositing = transferType == SavingsTransferType.Deposit;
            savingsAmountInput.Add(ActionInputFactory.CreateAmountSelector(available, transfer, clearTransferFeedback,
                depositing ? "Quick deposit · percentage of available cash" : "Quick withdrawal · percentage of savings",
                "Or enter a custom amount", depositing ? "Deposit" : "Withdraw"));
            RenderHistory(state, formatMoney);
            RenderCashFlow(finances, formatMoney, formatSignedMoney);
            RenderCredit(state, formatMoney, repayCredit);
            RenderInvesting(state, formatMoney, formatSignedMoney, invest);
            RenderProperties(state, formatMoney, formatSignedMoney, manageProperty);
            BankTabCredit.EnableInClassList("hidden", !adult);
            BankTabInvesting.EnableInClassList("hidden", !adult);
            BankTabProperty.EnableInClassList("hidden", !adult);
            return SelectTab(!adult ? BankTab.Savings : selectedTab);
        }

        public BankTab SelectTab(BankTab tab)
        {
            bankPanelSavings.EnableInClassList("hidden", tab != BankTab.Savings);
            bankPanelCredit.EnableInClassList("hidden", tab != BankTab.Credit);
            bankPanelInvesting.EnableInClassList("hidden", tab != BankTab.Investing);
            bankPanelProperty.EnableInClassList("hidden", tab != BankTab.Property);
            BankTabSavings.EnableInClassList("active", tab == BankTab.Savings);
            BankTabCredit.EnableInClassList("active", tab == BankTab.Credit);
            BankTabInvesting.EnableInClassList("active", tab == BankTab.Investing);
            BankTabProperty.EnableInClassList("active", tab == BankTab.Property);
            return tab;
        }

        public void ShowPropertyResult(bool succeeded, string summary)
        {
            FeedbackPresenter.ShowTransactionResult(propertyFeedback, succeeded, summary);
        }

        private void RenderProperties(GameState state, Func<long, string> money,
            Func<long, string> signed, Action<PropertyActionType, string> manage)
        {
            propertyActions.Clear();
            var portfolio = state.propertyPortfolio;
            var property = portfolio?.properties?.Find(item => item != null && item.status == "owned");
            if (property == null)
            {
                propertyPortfolioSummary.text = "Starter Rental Home · Price $75,000 · $15,000 down · $60,000 mortgage";
                propertyCashFlow.text = "Projected rent $750/month · Mortgage $400 · Tax $75 · Maintenance and vacancy vary.";
                var buy = new Button(() => manage(PropertyActionType.BuyStarterRental, null))
                    { text = "BUY STARTER RENTAL · $15,000 DOWN", name = "property-buy-starter" };
                buy.SetEnabled(state.finances.cashMinorUnits >= 1500000);
                propertyActions.Add(buy);
                return;
            }
            var equity = property.currentValueMinorUnits - property.mortgageBalanceMinorUnits;
            propertyPortfolioSummary.text = $"{property.displayName} · Value {money(property.currentValueMinorUnits)} · " +
                                            $"Mortgage {money(property.mortgageBalanceMinorUnits)} · Equity {signed(equity)}";
            propertyCashFlow.text = $"{(property.occupied ? "Occupied" : $"Vacant {property.vacancyMonths} month(s)")} · " +
                                    $"Condition {property.condition}/100 · Last income {money(property.lastIncomeMinorUnits)} · " +
                                    $"expenses {money(property.lastExpensesMinorUnits)}";
            AddPropertyButton("property-maintain", "MAINTAIN · $500",
                () => manage(PropertyActionType.Maintain, property.propertyId), property.condition < 100);
            if (!property.occupied)
                AddPropertyButton("property-find-tenant", "FIND TENANT",
                    () => manage(PropertyActionType.FindTenant, property.propertyId), property.condition >= 50);
            AddPropertyButton("property-sell", $"SELL · {signed(equity)} NET",
                () => manage(PropertyActionType.Sell, property.propertyId),
                equity >= 0 || state.finances.cashMinorUnits >= -equity);
        }

        private void AddPropertyButton(string name, string text, Action action, bool enabled)
        {
            var button = new Button(action) { name = name, text = text };
            button.AddToClassList("st-action-button");
            button.SetEnabled(enabled);
            propertyActions.Add(button);
        }

        private void RenderAccounts(FinancesState finances, bool adult, Func<long, string> money)
        {
            moneyAccountsList.Clear();
            moneyAccountsList.Add(UiComponentFactory.CreateAccountRow("cash-wallet", "💵", "Cash Wallet / Checking", money(finances.cashMinorUnits), "Checking detail · liquid cash available for actions and purchases"));
            moneyAccountsList.Add(UiComponentFactory.CreateAccountRow("savings", "🏦", "Savings", money(finances.savingsMinorUnits), $"{finances.savingsApyBasisPoints / 100m:0.00}% APY"));
            moneyAccountsList.Add(UiComponentFactory.CreateAccountRow("revolving-credit", "▣", "Revolving Credit", money(finances.householdCreditBalanceMinorUnits), $"{finances.householdCreditAprBasisPoints / 100m:0.00}% APR"));
            if (adult) moneyAccountsList.Add(UiComponentFactory.CreateAccountRow("index-fund", "↗", "Index Fund", money(finances.indexFundMinorUnits), "Long-term investment value"));
        }

        private void RenderFinancialState(FinancesState finances, Func<long, string> money)
        {
            var assets = finances.cashMinorUnits + finances.savingsMinorUnits + finances.indexFundMinorUnits;
            var netWorth = assets - finances.debtMinorUnits;
            var investmentPerformance = finances.indexFundMinorUnits - finances.indexFundContributionsMinorUnits;
            if (netWorth < 0)
                bankContextTip.text = $"Priority: debt exceeds assets by {money(-netWorth)}. Repay high-interest credit before adding investments.";
            else if (finances.lastNetCashFlowMinorUnits < 0)
                bankContextTip.text = $"Last month fell short by {money(-finances.lastNetCashFlowMinorUnits)}. Review expenses and protect available cash.";
            else if (finances.cashMinorUnits < finances.monthlyLivingExpensesMinorUnits)
                bankContextTip.text = "Cash is below one month of living expenses. Build a liquid buffer before investing.";
            else if (investmentPerformance < 0)
                bankContextTip.text = $"Your index fund is down {money(-investmentPerformance)} from contributions. Returns can remain negative; withdrawals are not forced.";
            else
                bankContextTip.text = "Finances are stable. Savings earns 3.50% APY; investing remains optional and can lose value.";
            bankContextTip.EnableInClassList("state-negative", netWorth < 0 || finances.lastNetCashFlowMinorUnits < 0);
            bankContextTip.EnableInClassList("state-extreme", assets >= 100000000000L || finances.debtMinorUnits >= 100000000000L);
            cashFlowNet.EnableInClassList("state-negative", finances.lastNetCashFlowMinorUnits < 0);
            indexFundPerformance.EnableInClassList("state-negative", investmentPerformance < 0);
        }

        private void RenderCashFlow(FinancesState finances, Func<long, string> money, Func<long, string> signed)
        {
            cashFlowGross.text = $"Gross income: {money(finances.lastGrossIncomeMinorUnits)}";
            cashFlowTaxes.text = $"Taxes: −{money(finances.lastTaxesMinorUnits)}";
            cashFlowExpenses.text = $"Expenses: −{money(finances.lastExpensesMinorUnits)}";
            cashFlowCreditInterest.text = $"Credit interest: −{money(finances.lastCreditInterestMinorUnits)}";
            cashFlowSavingsInterest.text = $"Savings interest: +{money(finances.lastSavingsInterestMinorUnits)}";
            cashFlowNet.text = $"Net change: {signed(finances.lastNetCashFlowMinorUnits)}";
            var projected = GameSessionService.CalculateProjectedAnnualSavingsInterest(finances);
            savingsProjection.text = $"{finances.savingsApyBasisPoints / 100m:0.00}% APY · about {money(projected)} interest over one year at the current balance; rate may change.";
        }

        private void RenderCredit(GameState state, Func<long, string> money, Action<long> repay)
        {
            var balance = state.finances.householdCreditBalanceMinorUnits;
            var limit = GameSessionService.CalculateHouseholdCreditLimit(state);
            creditBalanceValue.text = $"Balance: {money(balance)}";
            creditDetailValue.text = balance > 0 ? $"{state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR · Total debt {money(state.finances.debtMinorUnits)}" : $"No active revolving balance · Total debt {money(state.finances.debtMinorUnits)}";
            availableCreditValue.text = $"Available credit: {money(Math.Max(0, limit - state.finances.debtMinorUnits))} of {money(limit)}";
            creditRepaymentInput.Clear();
            creditRepaymentInput.Add(ActionInputFactory.CreateAmountSelector(Math.Min(state.finances.cashMinorUnits, balance), repay));
        }

        private void RenderInvesting(GameState state, Func<long, string> money, Func<long, string> signed, Action<long> invest)
        {
            var finances = state.finances;
            indexFundValue.text = $"Index fund: {money(finances.indexFundMinorUnits)}";
            indexFundContributions.text = $"Contributions: {money(finances.indexFundContributionsMinorUnits)}";
            indexFundPerformance.text = $"Market performance: {signed(finances.indexFundMinorUnits - finances.indexFundContributionsMinorUnits)}";
            var canInvest = GameSessionService.TryGetIndexInvestmentRequirement(state, out var requirement);
            indexInvestmentRequirement.text = canInvest ? "Unlocked · Minimum contribution $10" : $"Locked · {requirement}";
            indexInvestmentInput.Clear();
            if (canInvest) indexInvestmentInput.Add(ActionInputFactory.CreateAmountSelector(finances.cashMinorUnits, invest));
        }

        private void RenderHistory(GameState state, Func<long, string> money)
        {
            moneyTransactionHistory.Clear();
            var history = state.moneyTransactions;
            if (history == null || history.Count == 0) { moneyTransactionHistory.Add(new Label("No savings transfers yet.") { name = "money-history-empty" }); return; }
            var first = Math.Max(0, history.Count - 10);
            for (var index = history.Count - 1; index >= first; index--)
            {
                var entry = history[index];
                var name = entry.type == "savings_deposit" ? "Deposit" : entry.type == "savings_withdrawal" ? "Withdrawal" : entry.type == "credit_repayment" ? "Credit repayment" : "Savings interest";
                if (entry.type == "index_investment") name = "Index contribution"; else if (entry.type == "index_gain") name = "Index gain"; else if (entry.type == "index_loss") name = "Index loss";
                var row = new VisualElement(); row.AddToClassList("st-money-history-row");
                var title = new Label($"{name} · {money(entry.amountMinorUnits)}"); title.AddToClassList("st-money-history-title");
                var detail = new Label($"Age {entry.age}, month {entry.monthOfYear} · Cash {money(entry.cashBalanceMinorUnits)} · Savings {money(entry.savingsBalanceMinorUnits)}"); detail.AddToClassList("st-money-history-detail");
                row.Add(title); row.Add(detail); moneyTransactionHistory.Add(row);
            }
        }
    }
}
