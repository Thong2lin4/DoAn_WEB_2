namespace CoursesMVC.Models.DTO
{
    public class addCourses
    {
        public string? Title { get; set; } // Tên khóa học
        public string? Description { get; set; } // Mô tả chi tiết về khóa học
        public DateTime StartDate { get; set; } // Ngày bắt đầu khóa học
        public DateTime EndDate { get; set; } // Ngày kết thúc khóa học
        public string? Status { get; set; } // Trạng thái của khóa học (đang diễn ra, đã hoàn thành, v.v.)
        public List<int> StudentId { get; set; }
    }
}
