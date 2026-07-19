using System;
using System.Collections.Generic;

namespace StimTycoon.Runtime
{
    public enum ActionState { Ready, InProgress, Paused, Complete, Claimable, Expired, Locked }
    public enum ActionDestination { Life, Education, Money, Home, Social, Business, Goals }
    public enum ActionPaymentOption { None, Cash, Credit }

    [Serializable]
    public sealed class ActionDeltaPreview
    {
        public string targetId;
        public int amount;

        public ActionDeltaPreview(string targetId, int amount)
        {
            this.targetId = targetId;
            this.amount = amount;
        }
    }

    [Serializable]
    public sealed class ActionDefinition
    {
        public string id;
        public string title;
        public string description;
        public ActionDestination destination;
        public ActionState state;
        public string lockedReason;
        public long costMinorUnits;
        public List<ActionPaymentOption> paymentOptions = new List<ActionPaymentOption>();
        public List<ActionDeltaPreview> previews = new List<ActionDeltaPreview>();
        public int progress;
        public int progressRequired = 1;
        public int durationSeconds;
        public int claimWindowSeconds;
        public int cooldownMonths;
        public bool hasRisk;
    }

    public readonly struct ActionRequest
    {
        public string ActionId { get; }
        public string InstanceId { get; }
        public ActionPaymentOption PaymentOption { get; }

        public ActionRequest(
            string actionId,
            string instanceId,
            ActionPaymentOption paymentOption = ActionPaymentOption.None)
        {
            ActionId = actionId;
            InstanceId = instanceId;
            PaymentOption = paymentOption;
        }
    }
}
