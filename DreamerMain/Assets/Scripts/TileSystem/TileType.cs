public enum TileType
{
    Empty = 0,
    Full,

    SlopesStart,

    SlopeMid1 = SlopesStart,

    SlopeMid1F90Y,
    SlopeMid1F90XY,

    Slope45F90,
    Slope45F90X,

    Slope22P1,
    Slope22P1FX,

    Slope22P2,
    Slope22P2FX,

    Slope11P1,
    Slope11P1FX,

    Slope11P2,
    Slope11P2FX,

    Slope11P3,      
    Slope11P3FX,    

    Slope11P4,      
    Slope11P4FX,    

    OneWayStart,

    OneWaySlopeMid1FX = OneWayStart,      

    OneWaySlope45,       
    OneWaySlope45FX,    

    OneWaySlope22P1,     
    OneWaySlope22P1FX,  

    OneWaySlope22P2,    
    OneWaySlope22P2FX,  

    OneWaySlope11P1,    
    OneWaySlope11P1FX,  

    OneWaySlope11P2,    
    OneWaySlope11P2FX,  

    OneWaySlope11P3,   
    OneWaySlope11P3FX,  

    OneWaySlope11P4,    
    OneWaySlope11P4FX, 

    SlopeEnd = OneWaySlope11P4FX,

    OneWayFull, 

    OneWayEnd,

    Count,
}
