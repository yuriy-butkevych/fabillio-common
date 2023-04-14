using System;
using System.Collections.Generic;

namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Persons;

public class PersonResponse
{
    public DateTime? BirthDate { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public bool? IsDeceased { get; set; }
    public int? Gender { get; set; }
    public string NationalIdentificationNumber { get; set; }
    public string GivenName { get; set; }
    public string MiddleName { get; set; }
    public string FamilyName { get; set; }
    public DateTime? DateModified { get; set; }
    public List<Address> Addresses { get; set; }
    public string Email { get; set; }
    public string CellPhone { get; set; }
    public string HomePhone { get; set; }
    public string City { get; set; }
    public string CountryCode { get; set; }
    public string PersonKey { get; set; }
    public DateTime? DateAdded { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateUpdated { get; set; }
    public DateTime? SourceDateModified { get; set; }
    public DateTime? SourceDateCreated { get; set; }
    public string DisplayName { get; set; }
}

public class Address
{
    public int? Type { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string Municipality { get; set; }
    public string CountryCode { get; set; }
    public string PersonKey { get; set; }
}
