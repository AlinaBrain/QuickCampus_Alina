﻿using QuickCampus_Core.Interfaces;
using QuickCampus_DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCampus_Core.Services
{
    public class MstQualificationRepo : BaseRepository<BtprojecQuickcampusContext, MstQualification>,IMstQualificationRepo
    {
    }
}