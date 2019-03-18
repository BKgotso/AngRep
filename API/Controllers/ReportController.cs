using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using API.Models;
using System.Web.Http.Cors;
using System.IO;

using System.Web.Hosting;
using System.Net.Http.Headers;
using System.Data;


namespace API.Controllers
{
   [EnableCors(origins:"*",headers:"*",methods:"*")]
    public class ReportController : ApiController
    {
        [System.Web.Mvc.Route("api/Report/getReportData")]
        [HttpGet]
        public dynamic getReportData(int courseSelection)
        {
            SchoolDBEntities db = new SchoolDBEntities();
            db.Configuration.ProxyCreationEnabled = false;
            List<StudentGrade> grades;


            if(courseSelection == 1)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnsiteCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();
            }
            else if(courseSelection ==2)
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).Where(gr => db.OnlineCourses.Any(cc => cc.CourseID == gr.CourseID)).ToList();

            }
            else
            {
                grades = db.StudentGrades.Include(gg => gg.Person).Include(gg => gg.Course).Include(gg => gg.Course.Department).ToList();
            }


            return getExpandoReport(grades);
        }



        private dynamic getExpandoReport(List<StudentGrade> grades)
        {
            dynamic outObject = new ExpandoObject();
            var depList = grades.GroupBy(gg => gg.Course.Department.Name);
            List<dynamic> deps = new List<dynamic>();
            foreach(var group in depList)
            {
                dynamic department = new ExpandoObject();
                department.Name = group.Key;
                department.Average = group.Average(gg=> gg.Grade);
                deps.Add(department);
            }
            outObject.Departments = deps;

            var courseList = grades.GroupBy(gg=> gg.Course.Title);
            List<dynamic> courseGroups = new List<dynamic>();
            foreach(var group in courseList)
            {
                dynamic course = new ExpandoObject();
                course.Title = group.Key;
                course.Average = group.Average(gg=> gg.Grade);
                List<dynamic> flexiGrades = new List<dynamic>();
                foreach(var item in group)
                {
                    dynamic gradeObj = new ExpandoObject();
                    gradeObj.Student = item.Person.FirstName + " " + item.Person.LastName;
                    gradeObj.Course = item.Course.Title;
                    gradeObj.Grade = item.Grade;
                    flexiGrades.Add(gradeObj);
                }
                course.StudentGrades = flexiGrades;
                courseGroups.Add(course);
            }
            outObject.Courses = courseGroups;
            return outObject;
        }






    }
}
