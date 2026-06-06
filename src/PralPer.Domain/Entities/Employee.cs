using PralPer.Domain.Common;

namespace PralPer.Domain.Entities;

/// <summary>
/// HRMS-sourced employee master record. Read-only to the PER app (populated locally for dev;
/// later mapped to the Database-team's production schema). Modeled on the PRAL HRMS profile.
/// </summary>
public class Employee : AuditableEntity
{
    // Identity / codes
    public string HrCode { get; set; } = string.Empty;          // e.g. "3657" or "PRAL-EMP-234"
    public string AccountsCode { get; set; } = string.Empty;     // derived "ACC-{numeric HrCode}"
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }                            // Mr / Ms

    // Org structure
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int DesignationId { get; set; }
    public Designation? Designation { get; set; }
    public string? JobTitle { get; set; }                         // display title e.g. "Senior Software Engineer"
    public string? Wing { get; set; }
    public string? PayGroup { get; set; }                         // "Grade A - Level 3"
    public string? PayGrade { get; set; }
    public string? PayStep { get; set; }
    public string? EmploymentStatus { get; set; }                 // CONTRACTUAL / PERMANENT
    public string? PostingLocation { get; set; }

    // Reporting line
    public int? ReportingManagerId { get; set; }
    public Employee? ReportingManager { get; set; }
    public string? RmHrCode { get; set; }
    public string? RmName { get; set; }

    // Personal
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Cnic { get; set; }
    public DateTime? CnicExpiry { get; set; }
    public string? BloodGroup { get; set; }

    // Contact / address
    public string? WorkEmail { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? TelephoneNumber { get; set; }
    public string? HouseStreet { get; set; }
    public string? Area { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }

    // Evaluation-relevant history (bands on a 1–4 scale)
    public DateTime? RecruitmentDate { get; set; }
    public DateTime? LastPromotionDate { get; set; }
    public DateTime? LastIncrementDate { get; set; }
    public int? LastIncrementBand { get; set; }
    public DateTime? LastBonusDate { get; set; }
    public int? LastBonusBand { get; set; }
    public decimal? AttendancePercent { get; set; }

    public bool IsActive { get; set; } = true;

    // History (HRMS-sourced)
    public ICollection<PromotionHistory> Promotions { get; set; } = new List<PromotionHistory>();
    public ICollection<IncrementHistory> Increments { get; set; } = new List<IncrementHistory>();
    public ICollection<BonusHistory> Bonuses { get; set; } = new List<BonusHistory>();
}
