﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickCampus_Core.Common;
using QuickCampus_Core.Common.Helper;
using QuickCampus_Core.Interfaces;
using QuickCampus_Core.Services;
using QuickCampus_Core.ViewModel;
using QuickCampus_DAL.Context;
using System.Linq.Expressions;
using static QuickCampus_Core.Common.common;

namespace QuickCampusAPI.Controllers
{
    [Authorize(Roles = "Admin,Client,Client_User")]
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : Controller
    {
        private readonly IQuestion _questionrepo;
        private IConfiguration _config;
        private readonly IUserRepo _userRepo;
        private readonly IUserAppRoleRepo _userAppRoleRepo;
        private string _jwtSecretKey;
        private readonly ProcessUploadFile _uploadFile;
        private readonly IQuestionOptionRepo _questionOptionRepo;
        private IGroupRepo _groupRepo;
        private readonly ISectionRepo _sectionRepo;
        private readonly QuestionTypeRepo _questionTypeRepo;
        private readonly string _baseUrl;

        public QuestionController(IConfiguration configuration, IQuestion questionrepo,
            IUserAppRoleRepo userAppRoleRepo, IUserRepo userRepo,
            IQuestionOptionRepo questionOptionRepo, IGroupRepo groupRepo
            , ISectionRepo sectionRepo, QuestionTypeRepo questionTypeRepo)
        {
            _questionrepo = questionrepo;
            _config = configuration;
            _userRepo = userRepo;
            _userAppRoleRepo = userAppRoleRepo;
            _jwtSecretKey = _config["Jwt:Key"] ?? "";
            _questionOptionRepo = questionOptionRepo;
            _groupRepo = groupRepo;
            _sectionRepo = sectionRepo;
            _questionTypeRepo = questionTypeRepo;
            _baseUrl = _config.GetSection("APISitePath").Value;
        }


        [HttpGet]
        [Route("QuestionManage")]
        public async Task<ActionResult> GetAllQuestion(string? search, DataTypeFilter DataType, int pageStart = 1, int pageSize = 10)
        {
            IGeneralResult<List<GetAllQuestionViewModel>> result = new GeneralResult<List<GetAllQuestionViewModel>>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = (user.ClientId == null ? "0" : user.ClientId.ToString());
                }
                var newPageStart = 0;
                if (pageStart > 0)
                {
                    var startPage = 1;
                    newPageStart = (pageStart - startPage) * pageSize;
                }

                var questionTotalCount = 0;
                List<Question> questionList = new List<Question>();
                List<Question> questionData = new List<Question>();
                if (LoggedInUserRole != null && LoggedInUserRole.RoleId == (int)AppRole.Admin)
                {
                    questionData = _questionrepo.GetAllQuerable().Where(x => x.IsDeleted == false && ((DataType == DataTypeFilter.OnlyActive ? x.IsActive == true : (DataType == DataTypeFilter.OnlyInActive ? x.IsActive == false : true)))).ToList();
                }
                else
                {
                    questionData = _questionrepo.GetAllQuerable().Where(x => x.ClentId == Convert.ToInt32(LoggedInUserClientId) && x.IsDeleted == false && ((DataType == DataTypeFilter.OnlyActive ? x.IsActive == true : (DataType == DataTypeFilter.OnlyInActive ? x.IsActive == false : true)))).ToList();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                }
                questionList = questionData.Where(x => (x.Text.Contains(search ?? "", StringComparison.OrdinalIgnoreCase))).OrderByDescending(x => x.QuestionId).ToList();
                questionTotalCount = questionList.Count;
                questionList = questionList.Skip(newPageStart).Take(pageSize).ToList();
                //var response = questionList.Select(x => (GetAllQuestionViewModel)x).ToList();
                List<GetAllQuestionViewModel> data = new List<GetAllQuestionViewModel>();
                data.AddRange(questionList.Select(x => new GetAllQuestionViewModel
                {
                    QuestionId = x.QuestionId,
                    QuestionTypeId = x.QuestionTypeId,
                    SectionId = x.SectionId,
                    GroupId = x.GroupId,
                    Text = x.Text,
                    Marks = x.Marks
                }));
                foreach (var item in data)
                {
                    var optionData = _questionOptionRepo.GetAllQuerable().Where(y => y.QuestionId == item.QuestionId).Select(z => new QuestionsOptionVm
                    {
                        OptionText = z.OptionText,
                        OptionId = z.OptionId,
                        Imagepath = (string.IsNullOrEmpty(z.Imagepath) ? "" : Path.Combine(_baseUrl, z.Imagepath)),
                        IsCorrect = z.IsCorrect
                    }).ToList();
                    item.QuestionssoptionVm = optionData;
                }
                if (questionList.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Message = "Question get successfully";
                    result.Data = data;
                    result.TotalRecordCount = questionTotalCount;
                }
                else
                {
                    result.Message = "No applicant found!";
                }
            }
            catch (Exception ex)
            {
                result.Message = "server error! " + ex.Message;
            }
            return Ok(result);
        }
        
        [HttpGet]
        [Route("GetQuestionById")]
        public async Task<ActionResult> GetQuestionByid(int questionId)
        {
            IGeneralResult<QuestionTakeViewModel> result = new GeneralResult<QuestionTakeViewModel>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = user.ClientId.ToString();
                }
                if (questionId > 0)
                {
                    Question question = new Question();

                    if (LoggedInUserRole != null && LoggedInUserRole.RoleId == (int)AppRole.Admin)
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == questionId && x.IsDeleted == false).FirstOrDefault();
                    }
                    else
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == questionId && x.IsDeleted == false && x.ClentId == Convert.ToInt32(LoggedInUserClientId)).FirstOrDefault();
                    }

                    if (question == null)
                    {
                        result.Message = " Question does not exist";
                        return Ok(result);
                    }
                    result.Data = new QuestionTakeViewModel
                    {
                        QuestionId = question.QuestionId,
                        QuestionTypeId = question.QuestionTypeId ?? 0,
                        SectionId = question.SectionId ?? 0,
                        GroupId = question.GroupId ?? 0,
                        Text = question.Text,
                        Marks = question.Marks
                    };

                    var optiondata = _questionOptionRepo.GetAllQuerable().Where(y => y.QuestionId == question.QuestionId).Select(z => new QuestionsOptionVm
                    {
                        OptionText = z.OptionText,
                        OptionId = z.OptionId,
                        Imagepath = (string.IsNullOrEmpty(z.Imagepath) ? "" : Path.Combine(_baseUrl, z.Imagepath)),
                        IsCorrect = z.IsCorrect
                    }).ToList();
                    result.Data.QuestionssoptionVm = optiondata;
                    result.IsSuccess = true;
                    result.Message = "Question fetched successfully.";
                    return Ok(result);
                }
                else
                {
                    result.Message = "Please enter a valid Question Id.";
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server error! " + ex.Message;
            }
            return Ok(result);

        }

        [HttpGet]
        [Route("QuestionActiveInactive")]
        public async Task<ActionResult> QuestionActiveInactive(int QuestionId)
        {
            IGeneralResult<QuestionTakeViewModel> result = new GeneralResult<QuestionTakeViewModel>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = user.ClientId.ToString();
                }
                if (QuestionId > 0)
                {
                    Question question = new Question();

                    if (LoggedInUserRole != null && LoggedInUserRole.RoleId == (int)AppRole.Admin)
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == QuestionId && x.IsDeleted == false).FirstOrDefault();
                    }
                    else
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == QuestionId && x.IsDeleted == false && x.ClentId == Convert.ToInt32(LoggedInUserClientId)).FirstOrDefault();
                    }

                    if (question == null)
                    {
                        result.Message = " Question does not exist";
                        return Ok(result);
                    }
                    question.IsActive = !question.IsActive;
                    await _questionrepo.Update(question);
                    result.Data = new QuestionTakeViewModel
                    {
                        QuestionId = question.QuestionId,
                        QuestionTypeId = question.QuestionTypeId ?? 0,
                        SectionId = question.SectionId ?? 0,
                        GroupId = question.GroupId ?? 0,
                        Text = question.Text,
                        Marks = question.Marks
                    };

                    var optiondata = _questionOptionRepo.GetAllQuerable().Where(y => y.QuestionId == question.QuestionId).Select(z => new QuestionsOptionVm
                    {
                        OptionText = z.OptionText,
                        OptionId = z.OptionId,
                        Imagepath = (string.IsNullOrEmpty(z.Imagepath) ? "" : Path.Combine(_baseUrl, z.Imagepath)),
                        IsCorrect = z.IsCorrect
                    }).ToList();
                    result.Data.QuestionssoptionVm = optiondata;
                    result.IsSuccess = true;
                    result.Message = "Question Updated successfully.";
                    return Ok(result);
                }
                else
                {
                    result.Message = "Please enter a valid Question Id.";
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server error! " + ex.Message;
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("DeleteQuestionById")]
        public async Task<ActionResult> DeleteQuestion(int QuestionId)
        {
            IGeneralResult<QuestionTakeViewModel> result = new GeneralResult<QuestionTakeViewModel>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = user.ClientId.ToString();
                }
                if (QuestionId > 0)
                {
                    Question question = new Question();

                    if (LoggedInUserRole != null && LoggedInUserRole.RoleId == (int)AppRole.Admin)
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == QuestionId && x.IsDeleted == false).FirstOrDefault();
                    }
                    else
                    {
                        question = _questionrepo.GetAllQuerable().Where(x => x.QuestionId == QuestionId && x.IsDeleted == false && x.ClentId == Convert.ToInt32(LoggedInUserClientId)).FirstOrDefault();
                    }

                    if (question == null)
                    {
                        result.Message = " Question does not exist";
                        return Ok(result);
                    }
                    question.IsActive = false;
                    question.IsDeleted = true;
                    await _questionrepo.Update(question);

                    result.IsSuccess = true;
                    result.Message = "Question Deleted successfully.";
                    return Ok(result);
                }
                else
                {
                    result.Message = "Please enter a valid Question Id.";
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server error! " + ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("AddQuestion")]
        public async Task<ActionResult> AddQuestion([FromForm] QuestionTakeViewModel vm)
        {
            IGeneralResult<QuestionTakeViewModel> result = new GeneralResult<QuestionTakeViewModel>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = (user.ClientId == null ? "0" : user.ClientId.ToString());
                }
                if (!_groupRepo.Any(x => x.GroupId == vm.GroupId))
                {
                    result.Message = "Invalid group.";
                    return Ok(result);
                }
                if (!_sectionRepo.Any(x => x.SectionId == vm.SectionId))
                {
                    result.Message = "Invalid section.";
                    return Ok(result);
                }
                if (!_questionTypeRepo.Any(x => x.QuestionTypeId == vm.QuestionTypeId))
                {
                    result.Message = "Invalid question type.";
                    return Ok(result);
                }

                if (ModelState.IsValid)
                {
                    Question addQue = new Question
                    {
                        Text = vm.Text,
                        QuestionTypeId = vm.QuestionTypeId,
                        GroupId = vm.GroupId,
                        SectionId = vm.SectionId,
                        Marks = vm.Marks,
                        ClentId = Convert.ToInt32(LoggedInUserClientId),
                        IsActive = true,
                        IsDeleted = false,
                    };
                    var question = await _questionrepo.Add(addQue);
                    if (question.QuestionId > 0)
                    {
                        vm.QuestionId = question.QuestionId;
                        foreach (var option in vm.QuestionssoptionVm)
                        {
                            QuestionOption questionOption = new QuestionOption
                            {
                                QuestionId = vm.QuestionId,
                                OptionText = option.OptionText,
                                IsCorrect = option.IsCorrect,
                            };
                            if (option.Image != null)
                            {
                                var uploadImage = _uploadFile.GetUploadFile(option.Image);
                                if (!uploadImage.IsSuccess)
                                {
                                    result.Message = uploadImage.Message;
                                    return Ok(result);
                                }
                                questionOption.Imagepath = uploadImage.Data;
                                var addQueOpt = await _questionOptionRepo.Add(questionOption);
                                if (addQueOpt.OptionId == 0)
                                {
                                    result.Message = "something went wrong.";
                                    return Ok(result);
                                }
                                option.OptionId = addQueOpt.OptionId;
                                questionOption.Imagepath = Path.Combine(_baseUrl, questionOption.Imagepath);
                            }
                        }
                        result.IsSuccess = true;
                        result.Message = "Question added successfully.";
                        result.Data = vm;
                        return Ok(result);
                    }
                    else
                    {
                        result.Message = "something went wrong.";
                        return Ok(result);
                    }
                }
                else
                {
                    result.Message = GetErrorListFromModelState.GetErrorList(ModelState);
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server eroor. " + ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateQuestion")]
        public async Task<ActionResult> UpdateQuestion([FromBody] QuestionTakeViewModel vm)
        {

            IGeneralResult<QuestionTakeViewModel> result = new GeneralResult<QuestionTakeViewModel>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = (user.ClientId == null ? "0" : user.ClientId.ToString());
                }
                if (!_groupRepo.Any(x => x.GroupId == vm.GroupId))
                {
                    result.Message = "Invalid group.";
                    return Ok(result);
                }
                if (!_sectionRepo.Any(x => x.SectionId == vm.SectionId))
                {
                    result.Message = "Invalid section.";
                    return Ok(result);
                }
                if (!_questionTypeRepo.Any(x => x.QuestionTypeId == vm.QuestionTypeId))
                {
                    result.Message = "Invalid question type.";
                    return Ok(result);
                }
                if (vm.QuestionId > 0)
                {

                    if (ModelState.IsValid)
                    {
                        var questiondata = (await _questionrepo.GetAll(x => x.QuestionId == vm.QuestionId)).FirstOrDefault();
                        if (questiondata == null)
                        {
                            result.Message = "Question does Not Exist";
                            return Ok(result);
                        }

                        questiondata.Text = vm.Text;
                        questiondata.QuestionTypeId = vm.QuestionTypeId;
                        questiondata.GroupId = vm.GroupId;
                        questiondata.SectionId = vm.SectionId;
                        questiondata.Marks = vm.Marks;

                        questiondata.IsActive = true;
                        questiondata.IsDeleted = false;

                        var question = await _questionrepo.Update(questiondata);


                        var questionOptions = _questionOptionRepo.GetAllQuerable().Where(x => x.QuestionId == vm.QuestionId).ToList();
                        if (questionOptions.Count > 0)
                        {
                            foreach (var item in questionOptions)
                            {
                                await _questionOptionRepo.Delete(item);
                            }
                        }


                        foreach (var option in vm.QuestionssoptionVm)
                        {
                            QuestionOption questionOption = new QuestionOption
                            {
                                QuestionId = vm.QuestionId,
                                OptionText = option.OptionText,
                                IsCorrect = option.IsCorrect,
                            };
                            if (option.Image != null)
                            {
                                var uploadImage = _uploadFile.GetUploadFile(option.Image);
                                if (!uploadImage.IsSuccess)
                                {
                                    result.Message = uploadImage.Message;
                                    return Ok(result);
                                }
                                questionOption.Imagepath = uploadImage.Data;
                                var addQueOpt = await _questionOptionRepo.Add(questionOption);
                                if (addQueOpt.OptionId == 0)
                                {
                                    result.Message = "something went wrong.";
                                    return Ok(result);
                                }
                                option.OptionId = addQueOpt.OptionId;
                                questionOption.Imagepath = Path.Combine(_baseUrl, questionOption.Imagepath);
                            }
                        }
                        result.IsSuccess = true;
                        result.Message = "Question added successfully.";
                        result.Data = vm;
                        return Ok(result);
                    }
                }
                else
                {
                    result.Message = GetErrorListFromModelState.GetErrorList(ModelState);
                }


            }
            catch (Exception ex)
            {
                result.Message = "Server eroor. " + ex.Message;
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllQuestionTypes")]
        public async Task<ActionResult> GetAllQuestionTypes()
        {
            IGeneralResult<dynamic> result = new GeneralResult<dynamic>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = user.ClientId.ToString();
                }
                var questionTypes = _questionTypeRepo.GetAllQuerable().ToList();
                if (questionTypes.Count > 0)
                {
                    result.Data = questionTypes.Select(x => new
                    {
                        x.QuestionTypeId,
                        x.QuestionType1,

                    });
                    result.IsSuccess = true;
                    result.Message = "Question type fetched Successfully";
                    return Ok(result);
                }
                else
                {
                    result.Message = "No record Found!";
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server Error " + ex.Message;
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllSectionList")]
        public async Task<ActionResult> GetAllSectionList()
        {
            IGeneralResult<dynamic> result = new GeneralResult<dynamic>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = user.ClientId.ToString();
                }
                var sectionList = _sectionRepo.GetAllQuerable().ToList();
                if (sectionList.Count > 0)
                {
                    result.Data = sectionList.Select(x => new
                    {
                        x.SectionId,
                        x.Section1,

                    });
                    result.IsSuccess = true;
                    result.Message = "Section fetched Successfully";
                    return Ok(result);
                }
                else
                {
                    result.Message = "No record Found!";
                }
            }
            catch (Exception ex)
            {
                result.Message = "Server Error " + ex.Message;
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAllGroupsList")]
        public async Task<IActionResult> GetAllGroup()
        {
            IGeneralResult<List<GroupVm>> result = new GeneralResult<List<GroupVm>>();
            try
            {
                var LoggedInUserId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserClientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
                var LoggedInUserRole = (await _userAppRoleRepo.GetAll(x => x.UserId == Convert.ToInt32(LoggedInUserId))).FirstOrDefault();
                if (LoggedInUserClientId == null || LoggedInUserClientId == "0")
                {
                    var user = await _userRepo.GetById(Convert.ToInt32(LoggedInUserId));
                    LoggedInUserClientId = (user.ClientId == null ? "0" : user.ClientId.ToString());
                }
                List<Groupdl> groupList = new List<Groupdl>();
                groupList = _groupRepo.GetAllQuerable().ToList();

                var response = groupList.Select(x => (GroupVm)x).ToList();
                if (groupList.Count > 0)
                {
                    result.IsSuccess = true;
                    result.Message = "Groups fetched successfully";
                    result.Data = response;
                    result.TotalRecordCount = groupList.Count;
                }
                else
                {
                    result.Message = "Groups not found!";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Ok(result);
        }

       

    }
}
