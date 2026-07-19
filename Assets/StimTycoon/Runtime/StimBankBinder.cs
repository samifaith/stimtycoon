using System;
using StimTycoon.Saves;
using UnityEngine.UIElements;

namespace StimTycoon.Runtime
{
    internal sealed class StimBankBinder
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

        public StimBankBinder(VisualElement root)
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
            bankPanelSavings = root.Q<VisualElement>("bank-panel-savings");
            bankPanelCredit = root.Q<VisualElement>("bank-panel-credit");
            bankPanelInvesting = root.Q<VisualElement>("bank-panel-investing");
        }

        public Button SavingsDepositMode { get; }
        public Button SavingsWithdrawMode { get; }
        public Button BankTabSavings { get; }
        public Button BankTabCredit { get; }
        public Button BankTabInvesting { get; }

        public bool IsValid => savingsBalanceValue != null && savingsAvailableValue != null &&
            SavingsDepositMode != null && SavingsWithdrawMode != null && savingsAmountInput != null &&
            moneyTransactionHistory != null && moneyAccountsList != null && cashFlowGross != null &&
            cashFlowTaxes != null && cashFlowExpenses != null && cashFlowCreditInterest != null &&
            cashFlowSavingsInterest != null && cashFlowNet != null && savingsProjection != null &&
            creditBalanceValue != null && creditDetailValue != null && availableCreditValue != null &&
            creditRepaymentInput != null && indexFundValue != null && indexFundContributions != null &&
            indexFundPerformance != null && indexInvestmentRequirement != null && indexInvestmentInput != null &&
            BankTabSavings != null && BankTabCredit != null && BankTabInvesting != null &&
            bankPanelSavings != null && bankPanelCredit != null && bankPanelInvesting != null;

        public StimBankTab Render(StimGameState state, StimSavingsTransferType transferType, StimBankTab selectedTab,
            Func<long, string> formatMoney, Func<long, string> formatSignedMoney,
            Action<long> transfer, Action clearTransferFeedback, Action<long> repayCredit, Action<long> invest)
        {
            var adult = state.character.age >= 18;
            var finances = state.finances;
            savingsBalanceValue.text = formatMoney(finances.savingsMinorUnits);
            RenderAccounts(finances, adult, formatMoney);
            indexInvestmentInput.parent?.EnableInClassList("hidden", !adult);
            var available = transferType == StimSavingsTransferType.Deposit ? finances.cashMinorUnits : finances.savingsMinorUnits;
            savingsAvailableValue.text = transferType == StimSavingsTransferType.Deposit
                ? $"Available cash: {formatMoney(available)}" : $"Available savings: {formatMoney(available)}";
            SavingsDepositMode.EnableInClassList("active", transferType == StimSavingsTransferType.Deposit);
            SavingsWithdrawMode.EnableInClassList("active", transferType == StimSavingsTransferType.Withdrawal);
            savingsAmountInput.Clear();
            var depositing = transferType == StimSavingsTransferType.Deposit;
            savingsAmountInput.Add(StimActionInputFactory.CreateAmountSelector(available, transfer, clearTransferFeedback,
                depositing ? "Quick deposit · percentage of available cash" : "Quick withdrawal · percentage of savings",
                "Or enter a custom amount", depositing ? "Deposit" : "Withdraw"));
            RenderHistory(state, formatMoney);
            RenderCashFlow(finances, formatMoney, formatSignedMoney);
            RenderCredit(state, formatMoney, repayCredit);
            RenderInvesting(state, formatMoney, formatSignedMoney, invest);
            BankTabCredit.EnableInClassList("hidden", !adult);
            BankTabInvesting.EnableInClassList("hidden", !adult);
            return SelectTab(!adult ? StimBankTab.Savings : selectedTab);
        }

        public StimBankTab SelectTab(StimBankTab tab)
        {
            bankPanelSavings.EnableInClassList("hidden", tab != StimBankTab.Savings);
            bankPanelCredit.EnableInClassList("hidden", tab != StimBankTab.Credit);
            bankPanelInvesting.EnableInClassList("hidden", tab != StimBankTab.Investing);
            BankTabSavings.EnableInClassList("active", tab == StimBankTab.Savings);
            BankTabCredit.EnableInClassList("active", tab == StimBankTab.Credit);
            BankTabInvesting.EnableInClassList("active", tab == StimBankTab.Investing);
            return tab;
        }

        private void RenderAccounts(StimFinancesState finances, bool adult, Func<long, string> money)
        {
            moneyAccountsList.Clear();
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow("cash-wallet", "💵", "Cash Wallet", money(finances.cashMinorUnits), "Available for actions and purchases"));
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow("savings", "🏦", "Savings", money(finances.savingsMinorUnits), $"{finances.savingsApyBasisPoints / 100m:0.00}% APY"));
            moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow("revolving-credit", "▣", "Revolving Credit", money(finances.householdCreditBalanceMinorUnits), $"{finances.householdCreditAprBasisPoints / 100m:0.00}% APR"));
            if (adult) moneyAccountsList.Add(StimUiComponentFactory.CreateAccountRow("index-fund", "↗", "Index Fund", money(finances.indexFundMinorUnits), "Long-term investment value"));
        }

        private void RenderCashFlow(StimFinancesState finances, Func<long, string> money, Func<long, string> signed)
        {
            cashFlowGross.text = $"Gross income: {money(finances.lastGrossIncomeMinorUnits)}";
            cashFlowTaxes.text = $"Taxes: −{money(finances.lastTaxesMinorUnits)}";
            cashFlowExpenses.text = $"Expenses: −{money(finances.lastExpensesMinorUnits)}";
            cashFlowCreditInterest.text = $"Credit interest: −{money(finances.lastCreditInterestMinorUnits)}";
            cashFlowSavingsInterest.text = $"Savings interest: +{money(finances.lastSavingsInterestMinorUnits)}";
            cashFlowNet.text = $"Net change: {signed(finances.lastNetCashFlowMinorUnits)}";
            var projected = StimGameSessionService.CalculateProjectedAnnualSavingsInterest(finances);
            savingsProjection.text = $"{finances.savingsApyBasisPoints / 100m:0.00}% APY · about {money(projected)} interest over one year at the current balance; rate may change.";
        }

        private void RenderCredit(StimGameState state, Func<long, string> money, Action<long> repay)
        {
            var balance = state.finances.householdCreditBalanceMinorUnits;
            var limit = StimGameSessionService.CalculateHouseholdCreditLimit(state);
            creditBalanceValue.text = $"Balance: {money(balance)}";
            creditDetailValue.text = balance > 0 ? $"{state.finances.householdCreditAprBasisPoints / 100m:0.00}% APR · Total debt {money(state.finances.debtMinorUnits)}" : $"No active revolving balance · Total debt {money(state.finances.debtMinorUnits)}";
            availableCreditValue.text = $"Available credit: {money(Math.Max(0, limit - state.finances.debtMinorUnits))} of {money(limit)}";
            creditRepaymentInput.Clear();
            creditRepaymentInput.Add(StimActionInputFactory.CreateAmountSelector(Math.Min(state.finances.cashMinorUnits, balance), repay));
        }

        private void RenderInvesting(StimGameState state, Func<long, string> money, Func<long, string> signed, Action<long> invest)
        {
            var finances = state.finances;
            indexFundValue.text = $"Index fund: {money(finances.indexFundMinorUnits)}";
            indexFundContributions.text = $"Contributions: {money(finances.indexFundContributionsMinorUnits)}";
            indexFundPerformance.text = $"Market performance: {signed(finances.indexFundMinorUnits - finances.indexFundContributionsMinorUnits)}";
            var canInvest = StimGameSessionService.TryGetIndexInvestmentRequirement(state, out var requirement);
            indexInvestmentRequirement.text = canInvest ? "Unlocked · Minimum contribution $10" : $"Locked · {requirement}";
            indexInvestmentInput.Clear();
            if (canInvest) indexInvestmentInput.Add(StimActionInputFactory.CreateAmountSelector(finances.cashMinorUnits, invest));
        }

        private void RenderHistory(StimGameState state, Func<long, string> money)
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
