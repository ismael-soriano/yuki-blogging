using System.Xml.Serialization;

namespace Posts.Application.Common;

[XmlRoot("PagedResult")]
public class PagedResult<T>
{
    [XmlArray("Items")]
    [XmlArrayItem("Item")]
    public List<T> Items { get; set; } = new();

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    
    public PagedResult() {}
}