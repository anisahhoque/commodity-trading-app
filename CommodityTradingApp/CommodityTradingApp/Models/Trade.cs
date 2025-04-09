namespace CommodityTradingApp.Models
{

    public class Trade
    {
        public Guid TradeID { get; set; }        
        public Guid TraderID { get; set; }       
        public Guid CommodityID { get; set; }    
        public decimal PricePerUnit { get; set; }   
        public int Quantity { get; set; }       
        public bool IsBuy { get; set; }          
        public DateTime Expiry { get; set; }     
        public DateTime CreatedAt { get; set; }  
        public string Bourse { get; set; }       
        public Guid MitigationID { get; set; }   
        public bool IsOpen { get; set; }         
        public string Contract { get; set; }     
    }
    
}
