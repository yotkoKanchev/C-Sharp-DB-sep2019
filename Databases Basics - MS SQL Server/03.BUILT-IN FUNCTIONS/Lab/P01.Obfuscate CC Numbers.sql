SELECT 
	  CustomerID, 
	  FirstName, 
	  LastName, 
	  LEFT(PaymentNumber, 6) + REPLICATE('*', DATALENGTH(PaymentNumber) - 6) 
   AS PaymentNumber 
 FROM Customers

GO

CREATE VIEW v_PublicPaymentInfo AS
SELECT 
	  CustomerID, 
	  FirstName, 
	  LastName, 
	  LEFT(PaymentNumber, 6) + REPLICATE('*', DATALENGTH(PaymentNumber) - 6) 
   AS PaymentNumber 
 FROM Customers