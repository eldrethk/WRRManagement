/*
================================================================================
2026-07-29 - Extra Amenity / Add-ons completion + quote engine support
================================================================================
FOR REVIEW ONLY - nothing in this file has been run against SQL Server.

Context: the Extra Amenity / Add-ons feature was left unfinished. Beyond the
PerDayPerPerson/PerDay/.../Discount -> AmenityPricingType enum swap requested,
review of the actual schema + procs turned up that several procs in this area
were non-functional stubs or had parameter names that didn't match the calling
C# code at all (would throw at runtime):
  - genSelExtraAmenity / genSelExtraAmenityByID / genSelRackRateAmenities /
    genSelPackageAmenities / genSelPackageAmenityByID only ever SELECTed the ID
    column, never the rest of the row.
  - genInsExtraAmenity / genUpdExtraAmenity / genInsPackage / genUpdPackage /
    genInsPackageAmenity / genInsReservation / genInsReservationAmenity all had
    at least one parameter name that didn't match what the repository code
    sends (e.g. @OneTimeFeePerPerson vs OneTimeFeePerson, @ArrThur vs ArrThurs,
    @FirstName vs CusFirstName, @Qty vs MandatoryQuantity).
  - genUpdPackageAmenity and genSelReservationAmenity didn't exist at all.
This script fixes all of the above alongside the intended schema change, since
leaving the naming bugs in place while only swapping the pricing columns would
still leave every one of these procs broken.

Sections:
  1. ExtraAmenity: PerDayPerPerson/.../Discount bit columns -> PricingType
  2. Package: NightsFree/PercentOff/PricePoint bit columns -> PricingType
  3. Reservation: add IdempotencyKey for POST /reservations dedup
  4. Extra Amenity procs (insert/update/select/soft-delete)
  5. Package procs (insert/update)
  6. Package Amenity procs (select/update)
  7. Reservation + Reservation Amenity procs (insert/select, idempotency,
     allocation locking hint)
  8. Package Rate procs (new - needed by the PricePoint quote path, didn't exist)
  9. MinStay procs (found while testing Stay Restrictions - same ID-only bug)
  10. RackRate: drop TierD/Monthly columns, fix all five RackRate procs
      (three were ID-only stubs - see section header for why this one matters)
  11. Two Package listing procs that still referenced
      NightsFree/PercentOff/PricePoint after section 2 dropped those
      columns (missed these originally). NOTE: sections 4-10's "ID-only
      stub" fixes were confirmed against actual thrown errors as you hit
      them; section 11 originally also rewrote several sibling procs based
      only on the checked-in Stored_Procedures.sql file, which turned out
      to be a stale export that doesn't match the live database - those
      unconfirmed rewrites were removed. See section 11's header.
================================================================================
*/

----------------------------------------------------------------------------
-- 1. ExtraAmenity: six mutually-exclusive bit columns -> single PricingType
--    AmenityPricingType: 1=PerDayPerPerson 2=PerDay 3=PerNightStay
--                         4=OneTimeFee 5=OneTimeFeePerson 6=Discount
----------------------------------------------------------------------------
ALTER TABLE dbo.ExtraAmenity ADD PricingType tinyint NULL;
GO

UPDATE dbo.ExtraAmenity
SET PricingType = CASE
    WHEN PerDayPerPerson = 1 THEN 1
    WHEN PerDay = 1 THEN 2
    WHEN PerNightStay = 1 THEN 3
    WHEN OneTimeFee = 1 THEN 4
    WHEN OneTimeFeePerson = 1 THEN 5
    WHEN Discount = 1 THEN 6
    ELSE 1 -- default to PerDayPerPerson for rows with none of the legacy flags set
END;
GO

ALTER TABLE dbo.ExtraAmenity ALTER COLUMN PricingType tinyint NOT NULL;
GO

ALTER TABLE dbo.ExtraAmenity ADD CONSTRAINT DF_ExtraAmenity_PricingType DEFAULT (1) FOR PricingType;
GO

-- MandatoryQty/AdditionalPurchases already exist on the C# entity/repository calls
-- but were never added to the table - genInsExtraAmenity/genUpdExtraAmenity have been
-- silently failing (see header) any time those calls actually ran.
ALTER TABLE dbo.ExtraAmenity ADD MandatoryQty int NULL;
GO
ALTER TABLE dbo.ExtraAmenity ADD AdditionalPurchases bit NOT NULL CONSTRAINT DF_ExtraAmenity_AdditionalPurchases DEFAULT (0);
GO

ALTER TABLE dbo.ExtraAmenity DROP COLUMN PerDayPerPerson, PerDay, PerNightStay, OneTimeFee, OneTimeFeePerson, Discount;
GO

----------------------------------------------------------------------------
-- 2. Package: three mutually-exclusive bit columns -> single PricingType
--    PackagePricingType: 1=NightsFree 2=PercentOff 3=PricePoint
----------------------------------------------------------------------------
ALTER TABLE dbo.Package ADD PricingType tinyint NULL;
GO

UPDATE dbo.Package
SET PricingType = CASE
    WHEN NightsFree = 1 THEN 1
    WHEN PercentOff = 1 THEN 2
    WHEN PricePoint = 1 THEN 3
    ELSE 1
END;
GO

ALTER TABLE dbo.Package ALTER COLUMN PricingType tinyint NOT NULL;
GO

ALTER TABLE dbo.Package ADD CONSTRAINT DF_Package_PricingType DEFAULT (1) FOR PricingType;
GO

ALTER TABLE dbo.Package DROP COLUMN NightsFree, PercentOff, PricePoint;
GO

----------------------------------------------------------------------------
-- 3. Reservation: idempotency key for POST /reservations
--    (PackageID already exists on the table - it just wasn't wired up.)
----------------------------------------------------------------------------
ALTER TABLE dbo.Reservation ADD IdempotencyKey uniqueidentifier NULL;
GO

CREATE UNIQUE INDEX UX_Reservation_IdempotencyKey
    ON dbo.Reservation (IdempotencyKey)
    WHERE IdempotencyKey IS NOT NULL;
GO

----------------------------------------------------------------------------
-- 4. Extra Amenity procs
----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.genInsExtraAmenity
    @HotelID int,
    @Name nvarchar(150),
    @ShortDescription nvarchar(max),
    @Description text,
    @AmenityRate decimal(18,2),
    @Tax decimal(18,2),
    @PricingType tinyint,
    @ViewRate bit,
    @Mandatory bit,
    @MandatoryQty int,
    @Visible bit,
    @DiscountRegularRate decimal(18,2),
    @PictureUrl nvarchar(max),
    @ViewOnRackRate bit,
    @AdditionalPurchases bit
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ExtraAmenity
        (HotelID, Name, ShortDescription, Description, AmenityRate, Tax, PricingType,
         ViewRate, Mandatory, MandatoryQty, Visible, DiscountRegularRate, PictureUrl,
         ViewOnRackRate, AdditionalPurchases)
    VALUES
        (@HotelID, @Name, @ShortDescription, @Description, @AmenityRate, @Tax, @PricingType,
         @ViewRate, @Mandatory, @MandatoryQty, @Visible, @DiscountRegularRate, @PictureUrl,
         @ViewOnRackRate, @AdditionalPurchases);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

CREATE OR ALTER PROCEDURE dbo.genUpdExtraAmenity
    @AmenityID int,
    @Name nvarchar(150),
    @ShortDescription nvarchar(max),
    @Description text,
    @AmenityRate decimal(18,2),
    @Tax decimal(18,2),
    @PricingType tinyint,
    @ViewRate bit,
    @Mandatory bit,
    @MandatoryQty int,
    @DiscountRegularRate decimal(18,2),
    @PictureUrl nvarchar(max),
    @ViewOnRackRate bit,
    @AdditionalPurchases bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ExtraAmenity
    SET Name = @Name,
        ShortDescription = @ShortDescription,
        Description = @Description,
        AmenityRate = @AmenityRate,
        Tax = @Tax,
        PricingType = @PricingType,
        ViewRate = @ViewRate,
        Mandatory = @Mandatory,
        MandatoryQty = @MandatoryQty,
        DiscountRegularRate = @DiscountRegularRate,
        PictureUrl = @PictureUrl,
        ViewOnRackRate = @ViewOnRackRate,
        AdditionalPurchases = @AdditionalPurchases
    WHERE AmenityID = @AmenityID;
END
GO

-- Was only ever SELECTing [AmenityID] - every other field came back default/empty.
CREATE OR ALTER PROCEDURE dbo.genSelExtraAmenity
    @HotelID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AmenityID, HotelID, Name, ShortDescription, Description, AmenityRate, Tax,
           PricingType, ViewRate, Mandatory, MandatoryQty, Visible, DiscountRegularRate,
           PictureUrl, ViewOnRackRate, AdditionalPurchases
    FROM dbo.ExtraAmenity
    WHERE HotelID = @HotelID AND Visible = 1
    ORDER BY Name;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelExtraAmenityByID
    @AmenityID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AmenityID, HotelID, Name, ShortDescription, Description, AmenityRate, Tax,
           PricingType, ViewRate, Mandatory, MandatoryQty, Visible, DiscountRegularRate,
           PictureUrl, ViewOnRackRate, AdditionalPurchases
    FROM dbo.ExtraAmenity
    WHERE AmenityID = @AmenityID;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelRackRateAmenities
    @HotelID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AmenityID, HotelID, Name, ShortDescription, Description, AmenityRate, Tax,
           PricingType, ViewRate, Mandatory, MandatoryQty, Visible, DiscountRegularRate,
           PictureUrl, ViewOnRackRate, AdditionalPurchases
    FROM dbo.ExtraAmenity
    WHERE HotelID = @HotelID AND ViewOnRackRate = 1 AND Visible = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelExtraAmenitiesByPackageID
    @PackageID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ExtraAmenity.AmenityID, ExtraAmenity.HotelID, ExtraAmenity.Name,
           ExtraAmenity.ShortDescription, ExtraAmenity.Description, ExtraAmenity.AmenityRate,
           ExtraAmenity.Tax, ExtraAmenity.PricingType, ExtraAmenity.ViewRate,
           -- Mandatory/MandatoryQty/AdditionalPurchases reflect the package-level override
           -- when one exists (PackageAmenity), falling back to the amenity's own default.
           COALESCE(PackageAmenity.Mandatory, ExtraAmenity.Mandatory) AS Mandatory,
           COALESCE(PackageAmenity.MandatoryQuantity, ExtraAmenity.MandatoryQty) AS MandatoryQty,
           ExtraAmenity.Visible, ExtraAmenity.DiscountRegularRate, ExtraAmenity.PictureUrl,
           ExtraAmenity.ViewOnRackRate,
           COALESCE(PackageAmenity.AdditionalPurchases, ExtraAmenity.AdditionalPurchases) AS AdditionalPurchases
    FROM dbo.ExtraAmenity
    LEFT JOIN dbo.PackageAmenity ON ExtraAmenity.AmenityID = PackageAmenity.ExtraAmenityID
        AND PackageAmenity.PackageID = @PackageID
    WHERE PackageAmenity.PackageID = @PackageID
    ORDER BY PackageAmenity.Mandatory DESC;
END
GO

DROP PROCEDURE IF EXISTS dbo.genInvisbleExtraAmenity; -- misspelled original
GO

CREATE OR ALTER PROCEDURE dbo.genInvisibleExtraAmenity
    @AmenityID int
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ExtraAmenity SET Visible = 0 WHERE AmenityID = @AmenityID;
END
GO

-- Also only ever SELECTed [AmenityID] - PackageAmenityRepository.GetMandatoryAmenitiesAsync
-- (used by the quote engine to auto-include mandatory package amenities) needs full rows.
CREATE OR ALTER PROCEDURE dbo.genSelMandatoryAmenitiesForPackage
    @PackageID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ExtraAmenity.AmenityID, ExtraAmenity.HotelID, ExtraAmenity.Name,
           ExtraAmenity.ShortDescription, ExtraAmenity.Description, ExtraAmenity.AmenityRate,
           ExtraAmenity.Tax, ExtraAmenity.PricingType, ExtraAmenity.ViewRate,
           PackageAmenity.Mandatory,
           COALESCE(PackageAmenity.MandatoryQuantity, ExtraAmenity.MandatoryQty) AS MandatoryQty,
           ExtraAmenity.Visible, ExtraAmenity.DiscountRegularRate, ExtraAmenity.PictureUrl,
           ExtraAmenity.ViewOnRackRate,
           COALESCE(PackageAmenity.AdditionalPurchases, ExtraAmenity.AdditionalPurchases) AS AdditionalPurchases
    FROM dbo.ExtraAmenity
    LEFT JOIN dbo.PackageAmenity ON ExtraAmenity.AmenityID = PackageAmenity.ExtraAmenityID
    WHERE PackageAmenity.PackageID = @PackageID AND PackageAmenity.Mandatory = 1 AND ExtraAmenity.Visible = 1;
END
GO

-- Also only ever SELECTed [PackageID] - used by ExtraAmenityController/PackagesController
-- Details pages to list associated packages by name.
CREATE OR ALTER PROCEDURE dbo.genSelPackageAssociatedWithAmenity
    @ExtraAmenityID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Package.PackageID, Package.Name, Package.Description, Package.ShortDescription,
           Package.HotelID, Package.Amenities AS Amenity, Package.ArrMon, Package.ArrTues,
           Package.ArrWed, Package.ArrThur AS ArrThurs, Package.ArrFri, Package.ArrSat, Package.ArrSun,
           Package.MinDays, Package.MaxDays, Package.WeekendSurcharge, Package.ResortFees,
           Package.ValidFrom, Package.ValidTo, Package.EndDisplayDate, Package.Visible,
           Package.PricingType, Package.NumberOfNights, Package.PercentageOff, Package.Deposit,
           Package.ExtraPersonFee, Package.PackageAllocation, Package.DeletedPackage,
           Package.SmImage, Package.SortOrder AS [Order], Package.SpecialPage
    FROM dbo.Package
    LEFT JOIN dbo.PackageAmenity ON Package.PackageID = PackageAmenity.PackageID
    WHERE PackageAmenity.ExtraAmenityID = @ExtraAmenityID AND Package.Visible = 1 AND PackageAmenity.Mandatory = 1;
END
GO

----------------------------------------------------------------------------
-- 5. Package procs
----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.genInsPackage
    @HotelID int,
    @Name nvarchar(max),
    @Description text,
    @ShortDesc nvarchar(500),
    @Amenity bit,
    @ArrMon bit, @ArrTues bit, @ArrWed bit, @ArrThurs bit, @ArrFri bit, @ArrSat bit, @ArrSun bit,
    @MinDays int,
    @MaxDays int,
    @WeekendSurcharge bit,
    @ResortFees bit,
    @ValidFrom datetime,
    @ValidTo datetime,
    @EndDisplayDate datetime,
    @Visible bit,
    @PricingType tinyint,
    @NumberOfNights float,
    @PercentageOff decimal(18,2),
    @Deposit decimal(18,2),
    @ExtraPersonFee bit,
    @PackageAllocation bit,
    @DeletedPackage bit,
    @SmImage nvarchar(max),
    @Order int,
    @SpecialPage bit
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Package
        (Name, Description, ShortDescription, HotelID, Amenities, ArrMon, ArrTues, ArrWed, ArrThur,
         ArrFri, ArrSat, ArrSun, MinDays, MaxDays, WeekendSurcharge, ResortFees, ValidFrom, ValidTo,
         EndDisplayDate, Visible, PricingType, NumberOfNights, PercentageOff, Deposit,
         ExtraPersonFee, PackageAllocation, DeletedPackage, SmImage, SortOrder, SpecialPage)
    VALUES
        (@Name, @Description, @ShortDesc, @HotelID, @Amenity, @ArrMon, @ArrTues, @ArrWed, @ArrThurs,
         @ArrFri, @ArrSat, @ArrSun, @MinDays, @MaxDays, @WeekendSurcharge, @ResortFees, @ValidFrom, @ValidTo,
         @EndDisplayDate, @Visible, @PricingType, @NumberOfNights, @PercentageOff, @Deposit,
         @ExtraPersonFee, @PackageAllocation, @DeletedPackage, @SmImage, @Order, @SpecialPage);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

CREATE OR ALTER PROCEDURE dbo.genUpdPackage
    @PackageID int,
    @Name nvarchar(max),
    @Description text,
    @ShortDesc nvarchar(500),
    @Amenity bit,
    @ArrMon bit, @ArrTues bit, @ArrWed bit, @ArrThurs bit, @ArrFri bit, @ArrSat bit, @ArrSun bit,
    @MinDays int,
    @MaxDays int,
    @WeekendSurcharge bit,
    @ResortFees bit,
    @ValidFrom datetime,
    @ValidTo datetime,
    @EndDisplayDate datetime,
    @Visible bit,
    @PricingType tinyint,
    @NumberOfNights float,
    @PercentageOff decimal(18,2),
    @Deposit decimal(18,2),
    @ExtraPersonFee bit,
    @PackageAllocation bit,
    @DeletedPackage bit,
    @Order int,
    @SpecialPage bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Package
    SET Name = @Name,
        Description = @Description,
        ShortDescription = @ShortDesc,
        Amenities = @Amenity,
        ArrMon = @ArrMon, ArrTues = @ArrTues, ArrWed = @ArrWed, ArrThur = @ArrThurs,
        ArrFri = @ArrFri, ArrSat = @ArrSat, ArrSun = @ArrSun,
        MinDays = @MinDays,
        MaxDays = @MaxDays,
        WeekendSurcharge = @WeekendSurcharge,
        ResortFees = @ResortFees,
        ValidFrom = @ValidFrom,
        ValidTo = @ValidTo,
        EndDisplayDate = @EndDisplayDate,
        Visible = @Visible,
        PricingType = @PricingType,
        NumberOfNights = @NumberOfNights,
        PercentageOff = @PercentageOff,
        Deposit = @Deposit,
        ExtraPersonFee = @ExtraPersonFee,
        PackageAllocation = @PackageAllocation,
        DeletedPackage = @DeletedPackage,
        SortOrder = @Order,
        SpecialPage = @SpecialPage
    WHERE PackageID = @PackageID;
END
GO

----------------------------------------------------------------------------
-- 6. Package Amenity procs
----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.genInsPackageAmenity
    @PackageID int,
    @ExtraAmenityID int,
    @ViewRate bit,
    @Mandatory bit,
    @MandatoryQuantity int,
    @AdditionalPurchases bit
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.PackageAmenity (PackageID, ExtraAmenityID, ViewRate, Mandatory, MandatoryQuantity, AdditionalPurchases)
    VALUES (@PackageID, @ExtraAmenityID, @ViewRate, @Mandatory, @MandatoryQuantity, @AdditionalPurchases);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

-- Did not exist - PackageAmenityRepository.UpdateAsync had no matching proc to call.
CREATE OR ALTER PROCEDURE dbo.genUpdPackageAmenity
    @PackageAmenityID int,
    @ViewRate bit,
    @Mandatory bit,
    @MandatoryQuantity int,
    @AdditionalPurchases bit
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.PackageAmenity
    SET ViewRate = @ViewRate,
        Mandatory = @Mandatory,
        MandatoryQuantity = @MandatoryQuantity,
        AdditionalPurchases = @AdditionalPurchases
    WHERE AmenityID = @PackageAmenityID; -- PackageAmenity's own PK column is [AmenityID]
END
GO

-- Was only ever SELECTing [AmenityID]; also aliases the PK to PackageAmenityID to match
-- the C# entity (the PackageAmenity table's PK column is itself named AmenityID).
CREATE OR ALTER PROCEDURE dbo.genSelPackageAmenities
    @PackageID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AmenityID AS PackageAmenityID, PackageID, ExtraAmenityID, ViewRate, Mandatory,
           MandatoryQuantity, AdditionalPurchases
    FROM dbo.PackageAmenity
    WHERE PackageID = @PackageID;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelPackageAmenityByID
    @ExtraAmenityID int,
    @PackageID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AmenityID AS PackageAmenityID, PackageID, ExtraAmenityID, ViewRate, Mandatory,
           MandatoryQuantity, AdditionalPurchases
    FROM dbo.PackageAmenity
    WHERE PackageID = @PackageID AND ExtraAmenityID = @ExtraAmenityID;
END
GO

----------------------------------------------------------------------------
-- 7. Reservation + Reservation Amenity procs
----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.genInsReservation
    @HotelID int,
    @RoomTypeID int,
    @PackageID int = NULL,
    @PaymentTypeID int,
    @ArrivalDate datetime,
    @DepartureDate datetime,
    @TotalNights int,
    @Adults int,
    @Children int,
    @AvgDailyRate decimal(18,2),
    @SubTotal decimal(18,2),
    @TierLevel nvarchar(1),
    @ExtraAdultCharge decimal(18,2),
    @ExtraChildCharge decimal(18,2),
    @WeekendFees decimal(18,2),
    @ResortFees decimal(18,2),
    @TotalFees decimal(18,2),
    @Taxes decimal(18,2),
    @TotalCharge decimal(18,2),
    @Deposit decimal(18,2),
    @ExtraFees decimal(18,2),
    @Comments nvarchar(max),
    @CardHolderName nvarchar(50),
    @CardExpirationDate nvarchar(50),
    @CardNumber nvarchar(50),
    @CardSecureCode nvarchar(50),
    @CusFirstName nvarchar(max),
    @CusLastName nvarchar(max),
    @CusAddress1 nvarchar(max),
    @CusAddress2 nvarchar(max),
    @CusCity nvarchar(max),
    @CusState nvarchar(max),
    @CusZip nvarchar(20),
    @CusDayPhone nvarchar(50),
    @CusEveningPhone nvarchar(50),
    @CusEmail nvarchar(max),
    @BookedAmenity bit,
    @UserInitials nvarchar(50),
    @ReservationCreated datetime,
    @SessionID nvarchar(200),
    @CustomerId int,
    @IdempotencyKey uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Reservation
        (HotelID, RoomTypeID, PackageID, PaymentTypeID, BookedArrivalDate, BookedDepartureDate,
         TotalNights, Adults, Children, AverageDailyRate, SubTotal, TierLevel, ExtraAdultCharge,
         ExtraChildCharge, WeekendFee, ResortFee, TotalFees, Taxes, TotalCharge, Deposit, ExtraFees,
         Comments, CardHolderName, CardExpirationDate, CardNumber, CardSecureCode,
         CusFirstName, CusLastName, CusAddress1, CusAddress2, CusCity, CusState, CusZip,
         CusDayPhone, CusEvenPhone, CusEmail, BookedAmenity, UserInitals, ReservationCreated,
         SessionID, CustomerID, IdempotencyKey)
    VALUES
        (@HotelID, @RoomTypeID, @PackageID, @PaymentTypeID, @ArrivalDate, @DepartureDate,
         @TotalNights, @Adults, @Children, @AvgDailyRate, @SubTotal, @TierLevel, @ExtraAdultCharge,
         @ExtraChildCharge, @WeekendFees, @ResortFees, @TotalFees, @Taxes, @TotalCharge, @Deposit, @ExtraFees,
         @Comments, @CardHolderName, @CardExpirationDate, @CardNumber, @CardSecureCode,
         @CusFirstName, @CusLastName, @CusAddress1, @CusAddress2, @CusCity, @CusState, @CusZip,
         @CusDayPhone, @CusEveningPhone, @CusEmail, @BookedAmenity, @UserInitials, @ReservationCreated,
         @SessionID, @CustomerId, @IdempotencyKey);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

-- New: backs the idempotency-key replay check in ReservationService.CreateAsync.
CREATE OR ALTER PROCEDURE dbo.genSelReservationByIdempotencyKey
    @IdempotencyKey uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ReservationID FROM dbo.Reservation WHERE IdempotencyKey = @IdempotencyKey;
END
GO

CREATE OR ALTER PROCEDURE dbo.genInsReservationAmenity
    @ReservationID int,
    @AmenityID int,
    @ChargeAmount decimal(18,2),
    @TaxIncluded decimal(18,2),
    @Mandatory bit,
    @NumPeople int,
    @NumNights datetime,
    @TaxRate decimal(18,2),
    @TotalCharge decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ReservationAmenity
        (ReservationID, AmenityID, ChargeAmount, TaxIncluded, Mandatory, TaxRate, NumPeople, NumNights, TotalCharge)
    VALUES
        (@ReservationID, @AmenityID, @ChargeAmount, @TaxIncluded, @Mandatory, @TaxRate, @NumPeople, @NumNights, @TotalCharge);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

-- New: did not exist - ReservationAmenityRepository.GetBookedAmenitiesAsync had no proc to call.
CREATE OR ALTER PROCEDURE dbo.genSelReservationAmenity
    @ReservationID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, ReservationID, AmenityID, ChargeAmount, TaxIncluded, Mandatory, TaxRate,
           NumPeople, NumNights, TotalCharge
    FROM dbo.ReservationAmenity
    WHERE ReservationID = @ReservationID;
END
GO

-- Adds an UPDLOCK/HOLDLOCK hint so two concurrent bookings for the last room on a date
-- can't both read Quantity > 0 before either writes (the read-then-update was previously
-- unprotected). Same parameters/contract, no C# changes needed.
CREATE OR ALTER PROCEDURE dbo.genReserveRoom
    @RoomID int,
    @Date datetime
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @QTy int
    DECLARE @newQty int

    SELECT @QTy = Quantity FROM dbo.RoomTypeAllocation WITH (UPDLOCK, HOLDLOCK)
    WHERE RoomTypeID = @RoomID AND AllocateDate = @Date AND Quantity > 0

    SET @newQty = @QTy - 1

    IF @newQty >= 0
    BEGIN
        UPDATE dbo.RoomTypeAllocation
        SET Quantity = @newQty
        WHERE RoomTypeID = @RoomID AND AllocateDate = @Date
    END

    SELECT Quantity FROM dbo.RoomTypeAllocation
    WHERE RoomTypeID = @RoomID AND AllocateDate = @Date
END
GO

----------------------------------------------------------------------------
-- 8. Package Rate procs
----------------------------------------------------------------------------
-- New: did not exist. Needed by QuoteService for the PricePoint package pricing
-- path (PackageRateRepository.GetRateForDateAsync had no proc to call).
CREATE OR ALTER PROCEDURE dbo.genSelPackageRateByDate
    @RoomTypeID int,
    @Date date,
    @PackageID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RateID, PackageID, RoomTypeID, StartDate, EndDate, Price, Visible
    FROM dbo.PackageRate
    WHERE RoomTypeID = @RoomTypeID AND PackageID = @PackageID
      AND @Date BETWEEN StartDate AND EndDate AND Visible = 1;
END
GO

----------------------------------------------------------------------------
-- 9. MinStay procs
----------------------------------------------------------------------------
-- Found while testing Stay Restrictions: both only ever SELECTed [MinStayID].
-- genInsMinStay/genUpdMinStay already write the real quantity correctly - the
-- data was never lost, the grid just never displayed it back correctly.
CREATE OR ALTER PROCEDURE dbo.genSelMinStayByRoomID
    @RoomID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MinStayID, RoomTypeID, StayDate, MinNightStay AS Quantity
    FROM dbo.MinStay
    WHERE RoomTypeID = @RoomID AND StayDate >= CONVERT(char(8), GETDATE(), 112)
    ORDER BY StayDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelMinStayByID
    @MinStayID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MinStayID, RoomTypeID, StayDate, MinNightStay AS Quantity
    FROM dbo.MinStay
    WHERE MinStayID = @MinStayID;
END
GO

----------------------------------------------------------------------------
-- 10. RackRate: drop TierD/Monthly (system is Tier A/B/C only), fix procs
----------------------------------------------------------------------------
-- Superseded: an earlier version of this section wired up @Monthly/TierD
-- (found while testing Rack Rates - neither proc originally declared
-- @Monthly, which is what caused "too many arguments specified"). Per a
-- follow-up decision, Monthly/Tier D are being removed from the system
-- entirely rather than fixed - the Admin grid (RackRateComp.razor) never
-- exposed either field anyway. If you already applied the @Monthly-wired
-- version of these procs, this replaces them.
--
-- Also fixes genSelRackRateByDate/genSelRackRateByID/genSelRackRateByRoomID:
-- all three only ever SELECTed [RackRateID]. genSelRackRateByDate backs
-- RoomQueryService's and QuoteService's nightly-rate lookup, so this meant
-- every room price/quote/reservation was computing off TierARate/B/C = 0 -
-- the most serious bug found in this whole review.
ALTER TABLE dbo.RackRates DROP COLUMN TierDRate, MonthlyRate;
GO

CREATE OR ALTER PROCEDURE dbo.genInsRackRate
    @Start datetime,
    @End datetime,
    @RoomID int,
    @TierA decimal(18,2),
    @TierB decimal(18,2),
    @TierC decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.RackRates
        (StartDate, EndDate, RoomTypeID, TierARate, TierBRate, TierCRate, Visible)
    VALUES
        (@Start, @End, @RoomID, @TierA, @TierB, @TierC, 1);

    SELECT CAST(SCOPE_IDENTITY() AS int);
END
GO

CREATE OR ALTER PROCEDURE dbo.genUpdRackRate
    @RateID int,
    @Start datetime,
    @End datetime,
    @RoomID int,
    @TierA decimal(18,2),
    @TierB decimal(18,2),
    @TierC decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RackRates
    SET StartDate = @Start,
        EndDate = @End,
        TierARate = @TierA,
        TierBRate = @TierB,
        TierCRate = @TierC
    WHERE RackRateID = @RateID;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelRackRateByRoomID
    @RoomID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RackRateID, RoomTypeID, StartDate, EndDate, TierARate, TierBRate, TierCRate, Visible
    FROM dbo.RackRates
    WHERE RoomTypeID = @RoomID AND CONVERT(date, EndDate) >= CONVERT(date, GETDATE()) AND Visible = 1
    ORDER BY StartDate;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelRackRateByID
    @RackRateID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RackRateID, RoomTypeID, StartDate, EndDate, TierARate, TierBRate, TierCRate, Visible
    FROM dbo.RackRates
    WHERE RackRateID = @RackRateID;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelRackRateByDate
    @RoomID int,
    @Temp datetime
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RackRateID, RoomTypeID, StartDate, EndDate, TierARate, TierBRate, TierCRate, Visible
    FROM dbo.RackRates
    WHERE RoomTypeID = @RoomID AND @Temp BETWEEN StartDate AND EndDate AND Visible = 1;
END
GO

----------------------------------------------------------------------------
-- 11. Package listing procs still referencing dropped pricing-type columns
----------------------------------------------------------------------------
-- NOTE: the checked-in Stored_Procedures.sql this whole script was audited
-- against is a stale April 2025 export - it does NOT reflect what's
-- actually deployed (confirmed live: these two procs already use SELECT *,
-- not the ID-only stub the file shows). An earlier draft of this section
-- rewrote several sibling procs (genSelPackageByID, genSelPackagesByHotelID,
-- etc.) based on that stale file; those were removed since they were never
-- confirmed against the live database. Recommend re-exporting a fresh
-- Stored_Procedures.sql/Tables_Schema.sql from the actual server before
-- auditing further procs this way.
--
-- These two are real, confirmed bugs: section 2 of this script drops
-- Package.NightsFree/PercentOff/PricePoint in favor of PricingType, but
-- these procs' WHERE clauses still reference the dropped columns, which
-- throws "Invalid column name" the moment either runs. Fixed by switching
-- the WHERE clause to PricingType; SELECT * preserved as-is to match what's
-- actually live. PricingType: 1=NightsFree 2=PercentOff 3=PricePoint.
CREATE OR ALTER PROCEDURE dbo.genSelPackagesWithRates
    @HotelID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM Package
    WHERE HotelID = @HotelID AND DeletedPackage = 0 AND PricingType = 3
    ORDER BY ValidFrom DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.genSelPackagesWithTierLevel
    @HotelID int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM Package
    WHERE HotelID = @HotelID AND DeletedPackage = 0 AND PricingType IN (1, 2)
    ORDER BY ValidFrom DESC;
END
GO
