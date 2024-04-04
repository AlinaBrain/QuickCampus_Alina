﻿using System;
using System.Collections.Generic;

namespace QuickCampus_DAL.Context;

public partial class TblCollege
{
    public int CollegeId { get; set; }

    public string? CollegeName { get; set; }

    public string? CollegeCode { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public int? StateId { get; set; }

    public int? CountryId { get; set; }

    public string? Logo { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public string? ContectPerson { get; set; }

    public string? ContectPhone { get; set; }

    public string? ContectEmail { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ClientId { get; set; }

    public int? CityId { get; set; }

    public virtual MstCity? City { get; set; }

    public virtual ICollection<TblWalkInCollege> TblWalkInColleges { get; set; } = new List<TblWalkInCollege>();
}