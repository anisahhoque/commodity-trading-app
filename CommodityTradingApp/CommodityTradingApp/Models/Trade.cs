using System.ComponentModel.DataAnnotations;

namespace CommodityTradingApp.Models
{

    public class Trade
    {
        public Guid TradeId { get; set; }        
        public Guid TraderId { get; set; }       
        public Guid CommodityId { get; set; }    
        public decimal PricePerUnit { get; set; }   
        public int Quantity { get; set; }       
        public bool IsBuy { get; set; }          
        public DateTime Expiry { get; set; }     
        public DateTime CreatedAt { get; set; }
        [MaxLength(10)]
        public string Bourse { get; set; }       
        public Guid MitigationId { get; set; }   
        public bool IsOpen { get; set; }
        [MaxLength(5)]
        public string Contract { get; set; }

  
    }
    
}
