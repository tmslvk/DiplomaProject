using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaProject.Model
{
    public class Filling
    {
        //айди = день, просчет дней по расписанию, нужен,
        //расписание на вторник -> просчет всех возможных вторников -> запись по datetime.day = filling.id кол-во часов от дисциплин в расписании
        //возможность внесения выходных-будней, если выходной, то день в расписании не заполняется, сделать массив дат, которые будут исключаться из рабочих дней
        public int Id { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }  
        public int Hours { get; set; }
        public string Disciplenes { get; set; }
        public string TypeOfLesson { get; set; }


        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User User { get; set; }
       
    }
}
