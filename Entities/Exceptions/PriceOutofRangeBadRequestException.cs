namespace Entities.Exceptions
{
	public abstract partial class BadRequestException
	{
		public class PriceOutofRangeBadRequestException : BadRequestException
		{
			public PriceOutofRangeBadRequestException() 
				: base("Maximum Price should be less than 1000 and greater than 10.")
			{
				
			}
		}
	}
}
