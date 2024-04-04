﻿using QuickCampus_DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCampus_Core.ViewModel
{
    public class SkillsVm
    {
        public int SkillId { get; set; }

        public int? ApplicantSkillId { get; set; }

        public int? ApplicantId { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ClientId { get; set; }

        public virtual TblApplicant? Applicant { get; set; }
    
        public TblApplicantSkill ToSkillDbModel()
        {
            return new TblApplicantSkill
            { 
                ApplicantSkillId= ApplicantSkillId,
                ApplicantId=ApplicantId,
                IsActive=true,
                IsDeleted=false,
                ClientId=ClientId,
                CreatedDate=DateTime.UtcNow,
                ModifiedDate=SkillId >0?DateTime.UtcNow: null


            };
        }
        
    }
}