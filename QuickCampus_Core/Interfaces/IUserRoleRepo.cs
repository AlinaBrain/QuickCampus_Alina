﻿using QuickCampus_Core.Common;
using QuickCampus_DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCampus_Core.Interfaces
{
    public interface IUserRoleRepo 
    {
        Task<IGeneralResult<string>> SetClientAdminRole(int userId);
    }
}