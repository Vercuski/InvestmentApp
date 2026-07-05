using InvestmentApp.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace InvestmentApp.Domain.Entities;

[ExcludeFromCodeCoverage]
public sealed class SampleEntityDefinition : Entity
{
    [Key]
    public int SampleId1 { get; set; }
    [Required]
    public string? SampleString1 { get; set; }
    public bool SampleBoolean1 { get; set; }
    public int SampleInt1 { get; set; }
    public decimal SampleDecimal1 { get; set; }
}