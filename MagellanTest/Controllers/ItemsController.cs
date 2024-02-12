using Microsoft.AspNetCore.Mvc;

namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ItemsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public IActionResult CreateItem(ItemModel item)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new NpgsqlCommand("INSERT INTO item (item_name, parent_item, cost, req_date) VALUES (@itemName, @parentItem, @cost, @reqDate) RETURNING id", conn);
                    cmd.Parameters.AddWithValue("itemName", item.ItemName);
                    cmd.Parameters.AddWithValue("parentItem", item.ParentItem);
                    cmd.Parameters.AddWithValue("cost", item.Cost);
                    cmd.Parameters.AddWithValue("reqDate", item.ReqDate);

                    var id = cmd.ExecuteScalar();
                    return Ok(id);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetItem(int id)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new NpgsqlCommand("SELECT * FROM item WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var item = new ItemModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                ItemName = reader.GetString(reader.GetOrdinal("item_name")),
                                ParentItem = reader.IsDBNull(reader.GetOrdinal("parent_item")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_item")),
                                Cost = reader.GetInt32(reader.GetOrdinal("cost")),
                                ReqDate = reader.GetDateTime(reader.GetOrdinal("req_date"))
                            };
                            return Ok(item);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("totalcost/{itemName}")]
        public IActionResult GetTotalCost(string itemName)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new NpgsqlCommand("SELECT Get_Total_Cost(@itemName)", conn);
                    cmd.Parameters.AddWithValue("itemName", itemName);

                    var totalCost = (int?)cmd.ExecuteScalar();
                    return Ok(totalCost);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class ItemModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int? ParentItem { get; set; }
        public int Cost { get; set; }
        public DateTime ReqDate { get; set; }
    }
}
EOF

