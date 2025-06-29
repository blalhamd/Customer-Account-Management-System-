namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class CreateSecondryHolderDto
    {
        public string SecondaryClientId { get; set; } = null!;
        public bool EqualAccess { get; set; } 
    }
}

