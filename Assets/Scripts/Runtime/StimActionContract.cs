using System;
using System.Collections.Generic;

namespace StimTycoon.Runtime
{
    public enum StimActionState { Ready, InProgress, Complete, Claimable, Locked }
    public enum StimActionDestination { Life, Education, Money, Home, Social, Business, Goals }
    public enum StimActionPaymentOption { None, Cash, Credit }

    [Serializable]
    public sealed class StimActionDeltaPreview
    {
        public string targetId;
        public int amount;

        public StimActionDeltaPreview(string targetId, int amount)
        {
            this.targetId = targetId;
            this.amount = amount;
        }
    }

    [Serializable]
    public sealed class StimActionDefinition
    {
        public string id;
        public string title;
        public string description;
        public StimActionDestination destination;
        public StimActionState state;
        public string lockedReason;
        public long costMinorUnits;
        public List<StimActionPaymentOption> paymentOptions = new List<StimActionPaymentOption>();
        public List<StimActionDeltaPreview> previews = new List<StimActionDeltaPreview>();
        public int progress;
        public int progressRequired = 1;
        public int durationSeconds;
        public int cooldownMonths;
        public bool hasRisk;
    }

    public readonly struct StimActionRequest
    {
        public string ActionId { get; }
        public string InstanceId { get; }
        public StimActionPaymentOption PaymentOption { get; }

        public StimActionRequest(
            string actionId,
            string instanceId,
            StimActionPaymentOption paymentOption = StimActionPaymentOption.None)
        {
            ActionId = actionId;
            InstanceId = instanceId;
            PaymentOption = paymentOption;
        }
    }
}
