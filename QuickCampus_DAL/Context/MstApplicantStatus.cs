﻿using System;
using System.Collections.Generic;

namespace QuickCampus_DAL.Context;

public partial class MstApplicantStatus
{
    public int StatusId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? StatusName { get; set; }

    public virtual ICollection<TblApplicant> TblApplicants { get; set; } = new List<TblApplicant>();
}
