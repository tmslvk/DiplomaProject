using DiplomaProject.Model;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiplomaProject
{
    public partial class ScheduleForm : Form
    {
        int userId;
        DiplomaBDContext db;
        Schedule schedule;
        List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>()
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        Dictionary<DayOfWeek, string> dayDictionary = new Dictionary<DayOfWeek, string>()
        {
            {DayOfWeek.Monday, "Понедельник" },
            {DayOfWeek.Tuesday, "Вторник" },
            {DayOfWeek.Wednesday, "Среда" },
            {DayOfWeek.Thursday, "Четверг" },
            {DayOfWeek.Friday, "Пятница" },
            {DayOfWeek.Saturday, "Суббота" },

        };

        List<TimeSpan> timeOfLessons;

        public ScheduleForm(DiplomaBDContext diplomaBDContext, int id)
        {
            this.userId = id;
            this.db = diplomaBDContext;
            InitializeComponent();
        }

        private void ScheduleForm_Load(object sender, EventArgs e)
        {
            this.reportViewer1.RefreshReport();
            this.schedule = GetSchedule();
            this.timeOfLessons = GetTimeOfLessons(GetLessons());
            FillScheduleReport();

        }

        public Schedule GetSchedule()
        {
            Schedule newSchedule = new Schedule();
            foreach (var day in dayOfWeeks)
            {
                var lessons = GetLessonsByUserAndDay(day);
                newSchedule.Add(day, lessons.ConvertAll<ScheduleItem>(l => new ScheduleItem()
                {
                    Discipline = l.Discipline,
                    Group = l.Group,
                    Time = l.TimeOfLesson,
                    Place = l.PlaceOfLesson,
                    TypeOfLesson = l.TypeOfLesson,
                    TypeOfWeek = l.TypeOfWeek
                }));
            }
            return newSchedule;
        }

        public void FillScheduleReport()
        {
            foreach (var day in dayOfWeeks)
            {

                var lessons = schedule[day];
                ReportParameter dayReport = new ReportParameter("day", dayDictionary[day]);
                foreach (var time in timeOfLessons)
                {
                    var lesson = lessons.FirstOrDefault(l => l.Time == time);
                    if (lesson == null) continue;
                    ReportParameter typeOfWeek = new ReportParameter("typeOfWeek", lesson.TypeOfWeek);
                    ReportParameter typeOfLesson = new ReportParameter("typeOfLesson", lesson.TypeOfLesson);
                    ReportParameter place = new ReportParameter("place", lesson.Place);
                    ReportParameter group = new ReportParameter("group", lesson.Group);
                    ReportParameter discipline = new ReportParameter("dicipline", lesson.Discipline);
                    ReportParameter timeReport = new ReportParameter("time", lesson.Time.ToString());

                    reportViewer1.LocalReport.SetParameters(new ReportParameter[] { typeOfWeek, typeOfLesson, place, timeReport, group, discipline });
                    reportViewer1.RefreshReport();
                }
            }
        }

        public List<TimeSpan> GetTimeOfLessons(List<Lesson> lessons)
        {
            return lessons.Select(l => l.TimeOfLesson).OfType<TimeSpan>().Distinct().OrderBy(t => t).ToList();
        }
        public List<Lesson> GetLessons()
        {
            return db.Lessons.Where(l => l.UserId == userId).ToList();
        }

        public List<Lesson> GetLessonsByUserAndDay(DayOfWeek day)
        {
            return db.Lessons.Where(l => l.UserId == userId && l.DayOfWeek == (int)day).OrderBy(l => l.TimeOfLesson).ToList();
        }

        public class Schedule : Dictionary<DayOfWeek, List<ScheduleItem>>
        {

        }

        public class ScheduleItem
        {
            public string? Group;
            public string? TypeOfWeek;
            public string? Place;
            public string? TypeOfLesson;
            public string? Discipline;
            public TimeSpan? Time;
        }

    }
}
