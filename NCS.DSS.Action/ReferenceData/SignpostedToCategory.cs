
using System.ComponentModel;

namespace NCS.DSS.Action.ReferenceData
{
    public enum SignpostedToCategory
    {

        [Description("Charity")]
        Charity = 1,

        [Description("Training Provider")]
        TrainingProvider = 3,

        [Description("Apprenticeships")]
        ApprenticeshipService = 6,

        [Description("Specialist Organisation")]
        SpecialistOrganisation = 7,

        Employer = 9,

        [Description("Traineeships")]
        Traineeships = 10,

        [Description("Further Education")]
        FurtherEducation = 11,

        [Description("Higher Education")]
        HigherEducation = 12,

        [Description("Community Centres")]
        CommunityCentres = 13,
        
        [Description("Job Centre Plus WITH Data")]
        JCPWithData = 14,
        
        [Description("Job Centre Plus WITHOUT Data")]
        JCPWithoutData = 15,

        [Description("Not Applicable")]
        NotApplicable = 98,

        Other = 99
    }
}
