
using System.ComponentModel;

namespace NCS.DSS.Action.ReferenceData
{
    public enum SignpostedTo
    {

        [Description("Charity National")]
        CharityNational = 1,

        [Description("Charity Local")]
        CharityLocal = 2,

        [Description("Training Provider")]
        TrainingProvider = 3,

        College = 4,

        [Description("National Retraining Scheme")]
        NationalRetrainingScheme = 5,

        [Description("Apprenticeship Service")]
        ApprenticeshipService = 6,

        [Description("Specialist Organisation")]
        SpecialistOrganisation = 7,

        JCP = 8,
        Employer = 9,
        Other = 99
    }
}