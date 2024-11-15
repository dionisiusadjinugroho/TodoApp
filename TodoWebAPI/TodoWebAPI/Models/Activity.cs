using System;
using System.Collections.Generic;

namespace TodoWebAPI.Models;

public partial class Activity
{
    public int ActivityId { get; set; }

    public string ActivitiesNo { get; set; } = null!;

    public int UserId { get; set; }

    public string? Subject { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? CreatedDate { get; set; }

    public string? UpdatedDate { get; set; }
}
