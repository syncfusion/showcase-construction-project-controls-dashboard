using System.Globalization;
using Construction.Core.Entities;
using Construction.Infrastructure.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Seed;

public static class DatabaseSeeder
{
    // Real English text pools — no Bogus Lorem (which generates pseudo-Latin)
    private static readonly string[] SentencePool = new[]
    {
        "Structural steel delivery is scheduled for next week pending final approval.",
        "Electrical conduit routing must be coordinated with HVAC plenum spaces.",
        "Concrete compressive strength results meet the specified 4000 PSI requirement.",
        "Fire sprinkler rough-in inspection passed with no deficiencies noted.",
        "Window glazing samples have been submitted for architect review and approval.",
        "Underground utility locate has been completed and marked on-site.",
        "Elevator shaft alignment requires verification before next floor pour.",
        "HVAC ductwork is currently being installed on floors three through five.",
        "Emergency generator load test is scheduled for the following Monday.",
        "Roofing membrane installation is weather-dependent and subject to conditions.",
        "Interior framing is complete in the west wing and ready for inspection.",
        "Plumbing pressure test was successful and documentation has been submitted.",
        "Paint color samples have been approved by the owner representative.",
        "Site egress routes must remain clear throughout all phases of construction.",
        "Concrete slab on grade preparation is underway in the main assembly area.",
        "Structural column base plate settings have been surveyed and confirmed.",
        "Mechanical equipment pads have been formed and rebar is being placed.",
        "Temporary power distribution panel has been inspected and approved for use.",
        "Asbestos abatement documentation has been received and filed in the project records.",
        "Commissioning agent has been notified and will begin startup procedures shortly."
    };

    private static readonly string[] ShortDescPool = new[]
    {
        "Coordination meeting held with all subcontractors to review sequencing and conflict resolution.",
        "Material delivery delayed due to transportation logistics; updated schedule provided.",
        "Quality control inspection completed; all findings addressed and closed out.",
        "Submittal review cycle in progress; approval expected within standard timeframe.",
        "Site safety audit conducted; no major deficiencies identified; minor items corrected.",
        "Change order request pending review by owner's representative for scope adjustment.",
        "Commissioning tests scheduled for next phase; coordination with vendor representatives required.",
        "RFI response received from design team; field verification needed before proceeding.",
        "Pre-pour checklist completed and submitted for foundation concrete placement.",
        "Equipment startup sequence reviewed with operations staff and startup plan approved."
    };

    private static readonly string[] ParagraphPool = new[]
    {
        "The project team has completed the pre-construction survey and documented all existing conditions. Utility shutdowns have been coordinated with the local utility providers and temporary services established. The site trailer has been set up and is fully operational with internet and phone service available. The erosion control measures have been installed per the SWPPP and are inspected weekly. All workers have completed the required safety orientation and site-specific training before beginning work.",
        "Concrete placement for the foundation walls was completed successfully over three days. The cured specimens will be tested at seven and twenty-eight days to verify compressive strength. Formwork has been stripped and all surfaces are being prepared for waterproofing application. The waterproofing contractor is scheduled to begin work once the substrate moisture content is within acceptable limits. Inspection of the waterproofing installation will be performed before backfilling commences.",
        "The structural steel erection is progressing on schedule with all connections being made in accordance with the detailed connection tables. Fireproofing of steel members in the basement level has been completed and inspected. Steel deck for the second floor is being installed and will be followed by topping slab placement. Coordination with the mechanical and electrical trades is ongoing to resolve any overhead conflicts prior to deck concrete placement.",
        "The HVAC installation includes a central plant with chilled water and hot water generation, air handling units on each floor, and a variable air volume terminal system. Ductwork installation is progressing floor by floor starting at the basement and moving upward. Piping for the hydronic systems is being installed concurrently. TAB work will commence after all systems are substantially complete and will be performed by an independent testing agency.",
        "The electrical distribution system consists of a 277/480V main switchgear, motor control centers, panelboards, and a comprehensive raceway system throughout all floors. Conduit rough-in is complete in the basement and first floor. Wire pulling is underway in the completed conduit runs. Light fixtures are scheduled for delivery in four weeks and installation will follow immediately. Emergency and exit lighting has been roughed in and will be functional for the inspection."
    };

    private static readonly string[] LongParagraphPool = new[]
    {
        "The project scope includes complete demolition of the existing structure down to the foundation, selective salvage of reusable materials, and new construction of a three-story office building with underground parking. The construction sequence has been developed to minimize disruption to adjacent occupied facilities while maintaining safe access routes for pedestrians and emergency vehicles. Temporary shoring and underpinning of the existing foundation was required where new construction interfaces with existing footings. All demolition work is being performed in accordance with OSHA standards and daily safety inspections are conducted by the site safety manager.",
        "The mechanical systems design incorporates high-efficiency condensing boilers for heating, a water-cooled centrifugal chiller for cooling, and a building automation system for integrated control of all HVAC components. Energy recovery ventilators provide fresh air economizer cooling during shoulder seasons. The plumbing system includes domestic hot and cold water distribution, sanitary waste and vent piping, storm water management, and a wet pipe fire suppression system throughout. All mechanical equipment is selected to meet or exceed ASHRAE 90.1 energy efficiency standards and the owner has committed to pursuing LEED Gold certification for the project.",
        "The project schedule has been developed using critical path method scheduling with separate activity codes for each subcontractor scope of work. The schedule is updated monthly and reviewed in the weekly progress meeting with the owner's representative. Float time on non-critical activities has been consumed due to weather delays in the early excavation phase. The project team is evaluating acceleration options including overtime and second shift work to recover the lost time without impacting the substantial completion date. Schedule risk is being managed through regular review of long-lead material procurement and early identification of potential conflicts."
    };
    // Realistic construction task name pools
    private static readonly string[] TaskNamePool = new[]
    {
        "Structural Steel Erection",
        "Concrete Foundation Pour",
        "MEP Rough-In Coordination",
        "Fire Sprinkler Installation",
        "HVAC Ductwork Installation",
        "Electrical Conduit Routing",
        "Plumbing Stack Installation",
        "Roofing Membrane Application",
        "Exterior Facade Assembly",
        "Interior Framing",
        "Drywall Installation",
        "Paint and Finishes",
        "Flooring Installation",
        "Elevator Shaft Construction",
        "Site Utility Installation",
        "Parking Lot Paving",
        "Landscaping and Grading",
        "Window and Glazing",
        "Door Frame Installation",
        "Ceiling Grid Installation",
        "Fireproofing Application",
        "Waterproofing Installation",
        "Concrete Flatwork",
        "Rebar Placement",
        "Formwork Setup"
    };

    private static readonly string[] ChildTaskNamePool = new[]
    {
        "Survey and layout verification",
        "Material staging and handling",
        "Pre-pour inspection completed",
        "Rebar cage assembly",
        "Concrete placement and curing",
        "Formwork stripping",
        "Surface preparation",
        "Quality control inspection",
        "Load testing verification",
        "Coordination with other trades",
        "Progress documentation",
        "Safety compliance check",
        "Equipment setup and calibration",
        "Temporary support installation",
        "Final alignment verification",
        "Cleanup and turnover prep"
    };

    // Ordered construction lifecycle — milestones are seeded as a chronological subset of this
    // list so a project's schedule reads as a coherent sequence rather than shuffled phases.
    private static readonly string[] MilestoneSequencePool = new[]
    {
        "Notice to Proceed",
        "Design Development Complete",
        "Permit Approval",
        "Groundbreaking",
        "Foundation Complete",
        "Structural Steel Erected",
        "Building Dry-In",
        "MEP Rough-In Complete",
        "Interior Finishes Underway",
        "Substantial Completion",
        "Final Punch List",
        "Certificate of Occupancy"
    };

    private static readonly string[] SiteLocationNamePool = new[]
    {
        "Building A - Main Wing",
        "Building B - North Wing",
        "Central Plant",
        "Parking Structure",
        "Loading Dock",
        "Equipment Yard",
        "Temporary Trailer Complex",
        "Laydown Area North",
        "Laydown Area South",
        "Main Entrance Gate",
        "Substation Pad",
        "Water Meter Vault",
        "Transformer Area",
        "Mechanical Room",
        "Electrical Room"
    };

    private static readonly string[] CostItemDescPool = new[]
    {
        "Structural Steel - W10x49",
        "Ready-Mix Concrete 4000 PSI",
        "Rebar #5 Grade 60",
        "Plywood Sheathing 3/4\"",
        "2x6 Kiln-Dried Studs",
        "Fiberglass Batt Insulation R-19",
        "R-19 Fiberglass Insulation",
        "PVC Conduit 3/4\"",
        "Copper Pipe Type L 1/2\"",
        "HVAC Ductwork - 24\"x12\"",
        "Fire Sprinkler Heads",
        "Drywall 1/2\" Type X",
        "Interior Paint - Eggshell",
        "Acoustic Ceiling Tiles",
        "Carpet Tile - 24\"x24\"",
        "Exit Sign LED",
        "Circuit Breaker 20A",
        "Gypcrete Floor Underlayment",
        "Waterproofing Membrane",
        "Expansion Joint Filler"
    };

    private static readonly string[] VendorNamePool = new[]
    {
        "AECOM Constructors",
        "Turner Construction Co.",
        "Skanska USA Building",
        "Fluor Enterprises",
        "Bechtel Corporation",
        "Kiewit Corporation",
        "Mortenson Construction",
        "Gilbane Building Co.",
        "PCL Construction",
        "Clark Construction Group",
        "Balfour Beatty Construction",
        " Hensel Phelps Construction",
        "The Walsh Group",
        "Austin Industries",
        "Suffolk Construction Co."
    };

    private static readonly string[] DocFileNamePool = new[]
    {
        "Project_Schedule_Rev3",
        "Structural_Drawings_V2",
        "MEP_Coordination_CMeeting",
        "Submittal_Log_Update",
        "RFI_Response_Package",
        "Quality_Inspection_Report",
        "Safety_Audit_Summary",
        "Change_Order_Request_12",
        "Commissioning_Plan",
        "As-Built_Drawings_Draft",
        "Material_Delivery_Schedule",
        "Weekly_Progress_Report_08",
        "Shop_Drawing_Submittal",
        "Concrete_Mix_Design",
        "Fire_Protection_Plan"
    };

    private static readonly string[] SubmittalDescPool = new[]
    {
        "Structural Steel Shop Drawings - W14x90 Columns",
        "Ready-Mix Concrete Mix Design - 4000 PSI",
        "HVAC Equipment Submittals - Air Handling Units",
        "Electrical Panel Schedules - 277/480V Distribution",
        "Fire Sprinkler Shop Drawings - Wet Pipe System",
        "Plumbing Fixture Schedule - Low-Flow Fixtures",
        "Exterior Facade Samples - Curtain Wall System",
        "Roofing Membrane Product Data - TPO System",
        "Window and Glazing Schedule - Aluminum Frame",
        "Interior Finish Schedule - Paint and Wallcovering",
        "Elevator Shop Drawings - Hydraulic Passenger",
        "Acoustic Ceiling Tile Submittal - 2x4 Panels",
        "Door Hardware Schedule - Commercial Grade",
        "Lighting Fixture Cut Sheets - LED Interior",
        "Drywall Partition Details - STC Rating 50"
    };

    /// <summary>
    /// Short, title-style noun phrases shown as the "Title" column in the UI's
    /// Submittals table. Kept distinct from <see cref="SubmittalDescPool"/>
    /// (which holds the longer narrative used in <c>Description</c>). Each
    /// title pairs with the matching description by index so a single submittal
    /// reads coherently — e.g. Title "Structural Steel Shop Drawings" pairs
    /// with Description "Structural Steel Shop Drawings - W14x90 Columns".
    /// Lengths stay well under the 300-char <c>Title</c> column limit.
    /// </summary>
    private static readonly string[] SubmittalTitlePool = new[]
    {
        "Structural Steel Shop Drawings",
        "Ready-Mix Concrete Mix Design",
        "HVAC Equipment Submittal",
        "Electrical Panel Schedules",
        "Fire Sprinkler Shop Drawings",
        "Plumbing Fixture Schedule",
        "Exterior Facade Samples",
        "Roofing Membrane Product Data",
        "Window and Glazing Schedule",
        "Interior Finish Schedule",
        "Elevator Shop Drawings",
        "Acoustic Ceiling Tile Submittal",
        "Door Hardware Schedule",
        "Lighting Fixture Cut Sheets",
        "Drywall Partition Details"
    };

    private static readonly Func<int, string> PickSubmittalDesc = (idx) => SubmittalDescPool[idx % SubmittalDescPool.Length];
    private static readonly Func<int, string> PickSubmittalTitle = (idx) => SubmittalTitlePool[idx % SubmittalTitlePool.Length];

    private static readonly Func<int, string> PickTaskName = (idx) => TaskNamePool[idx % TaskNamePool.Length];
    private static readonly Func<int, string> PickChildTaskName = (idx) => ChildTaskNamePool[idx % ChildTaskNamePool.Length];
    private static readonly Func<int, string> PickSiteLocationName = (idx) => SiteLocationNamePool[idx % SiteLocationNamePool.Length];
    private static readonly Func<int, string> PickCostItemDesc = (idx) => CostItemDescPool[idx % CostItemDescPool.Length];
    private static readonly Func<int, string> PickVendorName = (idx) => VendorNamePool[idx % VendorNamePool.Length];
    private static readonly Func<int, string> PickDocFileName = (idx) => DocFileNamePool[idx % DocFileNamePool.Length];

    private static readonly Func<int, string> PickSentence = (idx) => SentencePool[idx % SentencePool.Length];
    private static readonly Func<int, string> PickShortDesc = (idx) => ShortDescPool[idx % ShortDescPool.Length];
    private static readonly Func<int, string> PickParagraph = (idx) => ParagraphPool[idx % ParagraphPool.Length];
    private static readonly Func<int, string> PickLongParagraph = (idx) => LongParagraphPool[idx % LongParagraphPool.Length];

    private static string RiskNumber(int projectIndex, int riskIndex) => $"RISK-{projectIndex:D2}-{riskIndex:D3}";

    private static int StatusProgress(ProjectStatus status, Faker faker)
    {
        return status switch
        {
            ProjectStatus.Planning => faker.Random.Int(0, 10),
            ProjectStatus.Active => faker.Random.Int(15, 85),
            ProjectStatus.OnHold => faker.Random.Int(20, 60),
            ProjectStatus.Completed => faker.Random.Int(95, 100),
            ProjectStatus.Cancelled => faker.Random.Int(10, 45),
            _ => faker.Random.Int(0, 100)
        };
    }

    private static Construction.Core.Entities.TaskStatus TaskStatusForProgress(int progress, ProjectStatus projectStatus, Faker faker)
    {
        if (projectStatus == ProjectStatus.Completed) return Construction.Core.Entities.TaskStatus.Completed;
        if (projectStatus == ProjectStatus.Planning) return faker.Random.Bool(0.7f) ? Construction.Core.Entities.TaskStatus.NotStarted : Construction.Core.Entities.TaskStatus.InProgress;
        if (projectStatus == ProjectStatus.Cancelled) return faker.Random.Bool(0.5f) ? Construction.Core.Entities.TaskStatus.Cancelled : Construction.Core.Entities.TaskStatus.OnHold;
        if (projectStatus == ProjectStatus.OnHold) return faker.Random.Bool(0.6f) ? Construction.Core.Entities.TaskStatus.OnHold : Construction.Core.Entities.TaskStatus.InProgress;
        if (progress == 0) return Construction.Core.Entities.TaskStatus.NotStarted;
        if (progress >= 100) return Construction.Core.Entities.TaskStatus.Completed;
        return faker.Random.Bool(0.8f) ? Construction.Core.Entities.TaskStatus.InProgress : Construction.Core.Entities.TaskStatus.OnHold;
    }

    private static int ChildProgressFromParent(int parentProgress, Faker faker)
    {
        var variance = faker.Random.Int(-15, 15);
        return Math.Clamp(parentProgress + variance, 0, 100);
    }

    public static async Task SeedAsync(ConstructionDbContext db, bool forceSeed = false, int projectCount = 25, CancellationToken ct = default)
    {
        // Deterministic seeding for reproducibility
        Randomizer.Seed = new Random(42);

        // Patch legacy rows that were created before metadata columns existed.
        await BackfillRfiSubmittalMetadataAsync(db, ct);

        // If already seeded, skip unless explicitly forced.
        if (!forceSeed && await db.Projects.AsNoTracking().AnyAsync(ct))
            return;

        // Clear all data so we can re-seed deterministically
        if (await db.Projects.AsNoTracking().AnyAsync(ct))
        {
            db.Risks.RemoveRange(db.Risks);
            db.Documents.RemoveRange(db.Documents);
            db.ChangeOrders.RemoveRange(db.ChangeOrders);
            db.Inspections.RemoveRange(db.Inspections);
            db.Submittals.RemoveRange(db.Submittals);
            db.RFIs.RemoveRange(db.RFIs);
            db.Tasks.RemoveRange(db.Tasks);
            db.Milestones.RemoveRange(db.Milestones);
            db.Budgets.RemoveRange(db.Budgets);
            db.SiteLocations.RemoveRange(db.SiteLocations);
            db.Projects.RemoveRange(db.Projects);
            await db.SaveChangesAsync(ct);
        }

        var faker = new Faker("en");

        // Index counters for deterministic English text selection
        int locIdx = 0, msIdx = 0, budIdx = 0, taskIdx = 0;
        int rfiIdx = 0, subIdx = 0, inspIdx = 0, coIdx = 0, docIdx = 0, riskIdx = 0;

        var projects = new List<Project>(projectCount);

        for (int i = 1; i <= projectCount; i++)
        {
            var start = faker.Date.BetweenDateOnly(DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-18)), DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)));
            var end = start.AddDays(faker.Random.Int(90, 540));

            var projectManager = faker.Person.FullName;
            var projectStatus = faker.PickRandom<ProjectStatus>();
            var targetProgress = StatusProgress(projectStatus, faker);

            var project = new Project
            {
                Name = $"{faker.Company.CompanyName()} Construction Site",
                Code = $"PRJ-{faker.Random.AlphaNumeric(6).ToUpper()}",
                Description = PickParagraph(i),
                StartDate = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                EndDate = end.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                Status = projectStatus,
                Location = $"{faker.Address.City()}, {faker.Address.StateAbbr()}",
                Budget = faker.Random.Decimal(1000000m, 50000000m), // $1M to $50M
                Manager = projectManager,
                CreatedDate = DateTime.UtcNow
            };

            // Site locations (maps)
            var baseLat = faker.Address.Latitude(33, 48);
            var baseLng = faker.Address.Longitude(-120, -70);
            var locations = new List<SiteLocation>();
            var projectLocationCount = faker.Random.Int(3, 7);
            foreach (var liOffset in Enumerable.Range(0, projectLocationCount))
            {
                var li = locIdx++;
                locations.Add(new SiteLocation
                {
                    Name = $"{project.Name} - {PickSiteLocationName(li)}",
                    Description = PickShortDesc(li),
                    Latitude = baseLat + faker.Random.Double(-0.05, 0.05),
                    Longitude = baseLng + faker.Random.Double(-0.05, 0.05),
                    LocationType = faker.PickRandom(new[] { "Building", "Trailer", "Laydown", "Parking", "Entrance" }),
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.Locations = locations;

            project.Milestones = GenerateProjectMilestones(project, projectStatus, targetProgress, ref msIdx, faker);

            // Tasks with hierarchy (Gantt) — derive progress/status from project state
            var tasks = new List<ProjectTask>();
            int rootCount = faker.Random.Int(6, 12);
            long totalTaskProgressTicks = 0;
            foreach (var r in Enumerable.Range(1, rootCount))
            {
                var rStart = faker.Date.Between(project.StartDate, project.EndDate.AddDays(-60));
                var rEnd = faker.Date.Between(rStart, project.EndDate);
                var ti = taskIdx++;
                var rootProgress = Math.Clamp(targetProgress + faker.Random.Int(-12, 12), 0, 100);
                var root = new ProjectTask
                {
                    Name = $"{PickTaskName(ti)} - Phase {(r % 4) + 1}",
                    Description = PickShortDesc(ti),
                    StartDate = rStart,
                    EndDate = rEnd,
                    Status = TaskStatusForProgress(rootProgress, projectStatus, faker),
                    Progress = rootProgress,
                    AssignedTo = faker.Person.FullName,
                    Dependencies = string.Empty,
                    CreatedDate = DateTime.UtcNow
                };
                totalTaskProgressTicks += rootProgress;

                // children
                var childCount = faker.Random.Int(3, 8);
                var childTasks = new List<ProjectTask>();
                foreach (var c in Enumerable.Range(1, childCount))
                {
                    var cStart = faker.Date.Between(root.StartDate, root.EndDate.AddDays(-7));
                    var cEnd = faker.Date.Between(cStart, root.EndDate);
                    var ci = taskIdx++;
                    var childProgress = ChildProgressFromParent(rootProgress, faker);
                    childTasks.Add(new ProjectTask
                    {
                        Name = $"{PickChildTaskName(ci)}",
                        Description = PickShortDesc(ci),
                        StartDate = cStart,
                        EndDate = cEnd,
                        Status = TaskStatusForProgress(childProgress, projectStatus, faker),
                        Progress = childProgress,
                        AssignedTo = faker.Person.FullName,
                        Dependencies = faker.Random.Bool() ? $"{faker.Random.Number(1, 5)}" : string.Empty,
                        CreatedDate = DateTime.UtcNow
                    });
                    totalTaskProgressTicks += childProgress;
                }
                root.SubTasks = childTasks;
                tasks.Add(root);
                tasks.AddRange(childTasks);
            }
            project.Tasks = tasks;
            project.Progress = tasks.Count > 0 ? (int)Math.Round((double)totalTaskProgressTicks / tasks.Count) : targetProgress;

            // Budgets and cost items — spent tied to task progress
            var budgets = new List<Budget>();
            var categories = new[] { "General Conditions", "Earthwork", "Concrete", "Steel", "MEP", "Finishes" };
            int costItemIdx = 0;
            decimal totalSpent = 0;
            foreach (var cat in faker.Random.ListItems(categories, faker.Random.Int(3, 6)).Distinct())
            {
                var allocated = faker.Random.Decimal(50_000, 1_000_000);
                var costItems = new List<CostItem>();
                var itemCount = faker.Random.Int(8, 22);
                foreach (var ciLocal in Enumerable.Range(1, itemCount))
                {
                    var progressFactor = (double)ciLocal / itemCount;
                    // distribute spend through project timeline up to current progress
                    var itemProgress = Math.Min(1.0, progressFactor * (project.Progress / 100.0) * faker.Random.Double(0.85, 1.15));
                    var amount = faker.Random.Decimal(500, 25_000) * (decimal)itemProgress;
                    if (amount <= 0) continue;
                    var cii = costItemIdx++;
                    var daysIntoProject = (int)((project.EndDate - project.StartDate).TotalDays * progressFactor * (project.Progress / 100.0));
                    var itemDate = project.StartDate.AddDays(daysIntoProject);
                    if (itemDate > DateTime.UtcNow) itemDate = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30));
                    costItems.Add(new CostItem
                    {
                        Description = PickCostItemDesc(cii),
                        Amount = Math.Round(amount, 2),
                        Date = itemDate,
                        Vendor = PickVendorName(cii),
                        Reference = faker.Random.Bool() ? $"INV-{faker.Random.Number(10000, 99999)}" : null,
                        CreatedDate = DateTime.UtcNow
                    });
                }
                var spent = costItems.Sum(ci => ci.Amount);
                totalSpent += spent;
                var bi = budIdx++;
                budgets.Add(new Budget
                {
                    Category = cat,
                    Description = PickShortDesc(bi),
                    AllocatedAmount = Math.Round(allocated, 2),
                    SpentAmount = Math.Round(spent, 2),
                    CostItems = costItems,
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.Budgets = budgets;

            // Normalize total spent to be proportional to project budget progress
            if (budgets.Count > 0 && project.Budget > 0)
            {
                var targetSpent = project.Budget * (project.Progress / 100m) * faker.Random.Decimal(0.85m, 1.10m);
                var currentSpent = budgets.Sum(b => b.SpentAmount);
                var scale = currentSpent > 0 ? targetSpent / currentSpent : 1m;
                foreach (var budget in budgets)
                {
                    foreach (var ci in budget.CostItems)
                    {
                        ci.Amount = Math.Round(ci.Amount * scale, 2);
                    }
                    budget.SpentAmount = Math.Round(budget.CostItems.Sum(ci => ci.Amount), 2);
                }
            }

            var rfiDisciplines = new[] { "Architectural", "Structural", "MEP", "Civil" };
            var rfiImpacts = new[] { "Schedule", "Cost", "Quality", "Safety", "Schedule · Cost" };

            // RFIs
            var rfis = new List<RFI>();
            foreach (var idx in Enumerable.Range(1, faker.Random.Int(5, 15)))
            {
                var ri = rfiIdx++;
                var submittedBy = faker.Random.Bool(0.7f) ? projectManager : faker.Person.FullName;
                var submittedDate = faker.Date.Between(project.StartDate, DateTime.UtcNow);
                var status = faker.PickRandom<RFIStatus>();
                rfis.Add(new RFI
                {
                    Number = $"RFI-{i:D2}-{idx:D3}",
                    Subject = PickSentence(ri),
                    Description = PickParagraph(ri),
                    Status = status,
                    SubmittedBy = submittedBy,
                    SubmittedDate = submittedDate,
                    AssignedTo = faker.Random.Bool() ? faker.Person.FullName : null,
                    ResponseDate = status == RFIStatus.Answered || status == RFIStatus.Closed
                        ? faker.Date.Between(submittedDate, DateTime.UtcNow)
                        : null,
                    Response = status == RFIStatus.Answered || status == RFIStatus.Closed
                        ? PickShortDesc(ri)
                        : null,
                    Discipline = faker.Random.ArrayElement(rfiDisciplines),
                    Impact = faker.Random.ArrayElement(rfiImpacts),
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.RFIs = rfis;

            var submittalDisciplines = new[] { "Architectural", "Structural", "MEP", "Civil", "Landscape" };
            var submittalTypes = new[] { "Shop Drawing", "Sample", "Mockup", "Data Sheet", "Certifications" };
            var specSections = new[] { "03100 Concrete", "05100 Structural Metal", "09200 Plaster", "15400 HVAC", "16000 Electrical", "26000 Fire Alarm" };

            // Submittals
            var submittals = new List<Submittal>();
            foreach (var idx in Enumerable.Range(1, faker.Random.Int(5, 15)))
            {
                var si = subIdx++;
                var submittedBy = faker.Random.Bool(0.7f) ? projectManager : faker.Person.FullName;
                var submittedDate = faker.Date.Between(project.StartDate, DateTime.UtcNow);
                var status = faker.PickRandom<SubmittalStatus>();
                var submittalType = faker.Random.ArrayElement(submittalTypes);
                submittals.Add(new Submittal
                {
                    Number = $"SUB-{i:D2}-{idx:D3}",
                    Title = PickSubmittalTitle(si),
                    Description = $"{PickSubmittalDesc(si)} - {submittalType}",
                    Status = status,
                    SubmittedBy = submittedBy,
                    SubmittedDate = submittedDate,
                    ReviewedBy = status >= SubmittalStatus.Approved ? faker.Person.FullName : null,
                    ReviewDate = status >= SubmittalStatus.Approved ? faker.Date.Between(submittedDate, DateTime.UtcNow) : null,
                    Comments = faker.Random.Bool() ? PickShortDesc(si) : null,
                    Discipline = faker.Random.ArrayElement(submittalDisciplines),
                    SpecificationSection = faker.Random.ArrayElement(specSections),
                    SubmittalType = submittalType,
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.Submittals = submittals;

            // Inspections (Scheduler) linked to locations where possible
            var inspections = new List<Inspection>();
            foreach (var _ in Enumerable.Range(1, faker.Random.Int(10, 20)))
            {
                var when = faker.Date.Between(project.StartDate, project.EndDate);
                var ii = inspIdx++;
                var status = faker.PickRandom<InspectionStatus>();
                inspections.Add(new Inspection
                {
                    Type = faker.PickRandom(new[] { "Safety", "Quality", "Structural", "Electrical", "Plumbing" }),
                    ScheduledDate = when,
                    CompletedDate = status == InspectionStatus.Passed || status == InspectionStatus.Failed
                        ? faker.Date.Between(when, project.EndDate)
                        : null,
                    Status = status,
                    Inspector = faker.Person.FullName,
                    Notes = faker.Random.Bool() ? PickShortDesc(ii) : null,
                    Findings = faker.Random.Bool() ? PickShortDesc(ii) : null,
                    LocationId = faker.Random.Bool(0.7f) && locations.Any() ? null /* assign later post-attach */ : null,
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.Inspections = inspections;

            // Change Orders
            var changeOrders = new List<ChangeOrder>();
            foreach (var idx in Enumerable.Range(1, faker.Random.Int(1, 5)))
            {
                var ci = coIdx++;
                var requestDate = faker.Date.Between(project.StartDate, DateTime.UtcNow);
                var status = faker.PickRandom<ChangeOrderStatus>();
                changeOrders.Add(new ChangeOrder
                {
                    Number = $"CO-{i:D2}-{idx:D2}",
                    Description = PickShortDesc(ci),
                    Amount = Math.Round(faker.Random.Decimal(1_000, 150_000), 2),
                    Status = status,
                    RequestedBy = faker.Random.Bool(0.7f) ? projectManager : faker.Person.FullName,
                    RequestDate = requestDate,
                    ApprovedBy = status == ChangeOrderStatus.Approved || status == ChangeOrderStatus.Implemented ? faker.Person.FullName : null,
                    ApprovalDate = status == ChangeOrderStatus.Approved || status == ChangeOrderStatus.Implemented ? faker.Date.Between(requestDate, DateTime.UtcNow) : null,
                    Justification = faker.Random.Bool() ? PickShortDesc(ci) : null,
                    ImpactDays = faker.Random.Bool() ? faker.Random.Int(0, 30) : null,
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.ChangeOrders = changeOrders;

            // Documents
            var documents = new List<Document>();
            foreach (var idx in Enumerable.Range(1, faker.Random.Int(2, 8)))
            {
                var di = docIdx++;
                var type = faker.PickRandom(new[] { "PDF", "DWG", "XLSX", "DOCX" });
                documents.Add(new Document
                {
                    FileId = Guid.NewGuid(),
                    FileName = $"{PickDocFileName(di)}.{type.ToLowerInvariant()}",
                    FileType = type,
                    FileSize = faker.Random.Long(50_000, 10_000_000),
                    DocumentType = faker.PickRandom(new[] { "Contract", "Drawing", "Report", "Spec" }),
                    Description = PickShortDesc(di),
                    UploadedBy = faker.Random.Bool(0.7f) ? projectManager : faker.Person.FullName,
                    UploadDate = faker.Date.Between(project.StartDate, DateTime.UtcNow),
                    CreatedDate = DateTime.UtcNow
                });
            }
            project.Documents = documents;

            // Risks
            var risks = GenerateProjectRisks(project, i, ref riskIdx, faker, projectStatus, project.StartDate, project.EndDate);
            project.Risks = risks;

            projects.Add(project);
        }

        await db.Projects.AddRangeAsync(projects, ct);
        await db.SaveChangesAsync(ct);

        // Patch existing RFIs and Submittals that were created before metadata columns existed
        await BackfillRfiSubmittalMetadataAsync(db, ct);

        // Now assign random LocationId to inspections after IDs exist
        var allLocations = await db.SiteLocations.AsNoTracking().ToListAsync(ct);
        if (allLocations.Count > 0)
        {
            var projectInspections = await db.Inspections.Include(i => i.Project).ToListAsync(ct);
            foreach (var insp in projectInspections)
            {
                var locs = allLocations.Where(l => l.ProjectId == insp.ProjectId).ToList();
                if (locs.Count == 0) continue;
                if (insp.LocationId == null)
                {
                    insp.LocationId = faker.PickRandom(locs).Id;
                }
            }
            await db.SaveChangesAsync(ct);
        }

        await BackfillRfiSubmittalMetadataAsync(db, ct);
    }

    private static async Task BackfillRfiSubmittalMetadataAsync(ConstructionDbContext db, CancellationToken ct = default)
    {
        var rfiDisciplines = new[] { "Architectural", "Structural", "MEP", "Civil" };
        var rfiImpacts = new[] { "Schedule", "Cost", "Quality", "Safety", "Schedule · Cost" };

        var rfisNeedingMetadata = await db.RFIs
            .Where(r => r.Discipline == null || r.Impact == null)
            .ToListAsync(ct);

        for (int i = 0; i < rfisNeedingMetadata.Count; i++)
        {
            var rfi = rfisNeedingMetadata[i];
            rfi.Discipline ??= rfiDisciplines[i % rfiDisciplines.Length];
            rfi.Impact ??= rfiImpacts[i % rfiImpacts.Length];
        }

        var submittalDisciplines = new[] { "Architectural", "Structural", "MEP", "Civil", "Landscape" };
        var submittalTypes = new[] { "Shop Drawing", "Sample", "Mockup", "Data Sheet", "Certifications" };
        var specSections = new[] { "03100 Concrete", "05100 Structural Metal", "09200 Plaster", "15400 HVAC", "16000 Electrical", "26000 Fire Alarm" };

        // Existing rows may lack a Title (the column was added in a later migration
        // and seeded with an empty default). Backfill any blank/missing Title with a
        // valid construction-submittal noun phrase so the UI's Title column is never
        // empty for legacy data. Index-rotation cycles through the title pool so
        // legacy submittals get varied, realistic titles rather than all the same.
        var submittalsNeedingMetadata = await db.Submittals
            .Where(s => s.Discipline == null || s.SubmittalType == null || s.SpecificationSection == null
                || s.Title == null || s.Title == string.Empty)
            .ToListAsync(ct);

        for (int i = 0; i < submittalsNeedingMetadata.Count; i++)
        {
            var submittal = submittalsNeedingMetadata[i];
            submittal.Discipline ??= submittalDisciplines[i % submittalDisciplines.Length];
            submittal.SubmittalType ??= submittalTypes[i % submittalTypes.Length];
            submittal.SpecificationSection ??= specSections[i % specSections.Length];
            if (string.IsNullOrWhiteSpace(submittal.Title))
            {
                submittal.Title = PickSubmittalTitle(i);
            }
        }

        if (rfisNeedingMetadata.Count > 0 || submittalsNeedingMetadata.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Seeds a project's milestones as a chronological subset of the standard construction
    /// lifecycle, with dates and statuses that stay consistent with "today" and the project's
    /// status/progress — rather than independently-randomized dates and statuses, which
    /// previously produced out-of-order titles and, worse, meant nearly every project's
    /// "upcoming milestones" window came back empty once its seeded StartDate/EndDate span
    /// had fully elapsed. Active/Planning/OnHold projects are guaranteed at least one
    /// milestone in the near future so the Overview/Schedule "upcoming" views always have data.
    /// </summary>
    private static List<Milestone> GenerateProjectMilestones(Project project, ProjectStatus projectStatus, int targetProgress, ref int msIdx, Faker faker)
    {
        var count = faker.Random.Int(4, 8);

        // Evenly-spaced picks across the lifecycle pool so a shorter list still reads as a
        // sensible subset of phases rather than always the first N.
        var names = new List<string>();
        for (int idx = 0; idx < count; idx++)
        {
            var poolIndex = count == 1 ? 0 : (int)Math.Round(idx * (MilestoneSequencePool.Length - 1.0) / (count - 1));
            names.Add(MilestoneSequencePool[poolIndex]);
        }
        names = names.Distinct().ToList();

        var now = DateTime.UtcNow;
        bool isOngoing = projectStatus is ProjectStatus.Active or ProjectStatus.Planning or ProjectStatus.OnHold;
        var milestones = new List<Milestone>();

        if (isOngoing)
        {
            // Split the sequence into "already happened" vs. "still ahead" using the project's
            // target progress, always leaving at least one milestone in the future so the
            // upcoming-milestones views have something to show regardless of when this is queried.
            var pastCount = Math.Clamp((int)Math.Round(names.Count * (targetProgress / 100.0)), 0, names.Count - 1);
            var futureCount = names.Count - pastCount;

            // Stretch the planning horizon out from "today" in case the seeded EndDate has
            // already drifted into the past for a project that's nominally still active.
            var horizonEnd = project.EndDate < now.AddDays(60) ? now.AddDays(faker.Random.Int(90, 240)) : project.EndDate;

            for (int idx = 0; idx < pastCount; idx++)
            {
                var mi = msIdx++;
                var span = (now - project.StartDate).TotalDays;
                var frac = (idx + 1.0) / (pastCount + 1.0);
                var mDate = project.StartDate.AddDays(Math.Max(0, span) * frac * faker.Random.Double(0.9, 1.0));

                // The most recent past phase reflects a paused project rather than finished work.
                var isLastPast = idx == pastCount - 1;
                var status = isLastPast && projectStatus == ProjectStatus.OnHold && faker.Random.Bool(0.5f)
                    ? Construction.Core.Entities.TaskStatus.OnHold
                    : Construction.Core.Entities.TaskStatus.Completed;

                milestones.Add(new Milestone
                {
                    Name = names[idx],
                    Description = PickShortDesc(mi),
                    Date = mDate,
                    Status = status,
                    CreatedDate = DateTime.UtcNow
                });
            }

            for (int idx = 0; idx < futureCount; idx++)
            {
                var mi = msIdx++;
                DateTime mDate;
                if (idx == 0)
                {
                    // Guarantee a near-term milestone (within the next ~3-4 weeks) so both the
                    // 14-day dashboard window and the 30-day project-detail window pick it up.
                    mDate = now.AddDays(faker.Random.Int(3, 25));
                }
                else
                {
                    var span = Math.Max(1, (horizonEnd - now.AddDays(25)).TotalDays);
                    var frac = idx / (double)futureCount;
                    mDate = now.AddDays(25).AddDays(span * frac * faker.Random.Double(0.9, 1.1));
                }

                var status = idx == 0 && projectStatus == ProjectStatus.Active && faker.Random.Bool(0.5f)
                    ? Construction.Core.Entities.TaskStatus.InProgress
                    : Construction.Core.Entities.TaskStatus.NotStarted;

                milestones.Add(new Milestone
                {
                    Name = names[pastCount + idx],
                    Description = PickShortDesc(mi),
                    Date = mDate,
                    Status = status,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        else
        {
            // Completed/Cancelled projects are fully historical — no upcoming milestones expected,
            // which is a correct empty state rather than a bug.
            var historicalEnd = projectStatus == ProjectStatus.Completed && project.EndDate > now ? now : project.EndDate;
            var span = Math.Max(0, (historicalEnd - project.StartDate).TotalDays);
            var cancelledFromIndex = projectStatus == ProjectStatus.Cancelled
                ? (int)Math.Ceiling(names.Count * faker.Random.Double(0.4, 0.75))
                : names.Count;

            for (int idx = 0; idx < names.Count; idx++)
            {
                var mi = msIdx++;
                var frac = (idx + 1.0) / (names.Count + 1.0);
                var mDate = project.StartDate.AddDays(span * frac);
                var status = projectStatus == ProjectStatus.Completed
                    ? Construction.Core.Entities.TaskStatus.Completed
                    : idx >= cancelledFromIndex
                        ? Construction.Core.Entities.TaskStatus.Cancelled
                        : Construction.Core.Entities.TaskStatus.Completed;

                milestones.Add(new Milestone
                {
                    Name = names[idx],
                    Description = PickShortDesc(mi),
                    Date = mDate,
                    Status = status,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        return milestones;
    }

    private static List<Risk> GenerateProjectRisks(Project project, int projectIndex, ref int riskIdx, Faker faker, ProjectStatus projectStatus, DateTime projectStart, DateTime projectEnd)
    {
        // Distribute a realistic number of risks per project while hitting the target portfolio distribution.
        int riskCount = projectStatus switch
        {
            ProjectStatus.Active => faker.Random.Int(2, 5),
            ProjectStatus.OnHold => faker.Random.Int(3, 6),
            ProjectStatus.Planning => faker.Random.Int(1, 3),
            ProjectStatus.Completed => faker.Random.Int(0, 2),
            ProjectStatus.Cancelled => faker.Random.Int(1, 3),
            _ => faker.Random.Int(1, 4)
        };

        var risks = new List<Risk>();
        for (int r = 1; r <= riskCount; r++)
        {
            var ri = riskIdx++;
            var identified = faker.Date.Between(projectStart, projectEnd);
            var severity = SeverityForPortfolioTarget(ri, projectStatus);
            var probability = faker.PickRandom<RiskProbability>();
            var impactType = faker.PickRandom<RiskImpactType>();
            var status = RiskStatusForSeverity(severity, faker);
            var targetResolution = faker.Date.Between(identified, projectEnd.AddDays(30));
            var closedDate = status == RiskStatus.Mitigated
                ? faker.Date.Between(identified, targetResolution)
                : (DateTime?)null;

            risks.Add(new Risk
            {
                ProjectId = project.Id,
                Number = RiskNumber(projectIndex, r),
                Title = PickSentence(ri).TrimEnd('.'),
                Description = PickShortDesc(ri),
                Severity = severity,
                Probability = probability,
                ImpactType = impactType,
                ImpactDescription = impactType == RiskImpactType.Cost
                    ? "Potential cost overrun due to material escalation."
                    : impactType == RiskImpactType.Schedule
                        ? "Schedule slippage from long-lead procurement."
                        : "Quality or safety impact requiring inspection.",
                ImpactCost = impactType == RiskImpactType.Cost ? Math.Round(faker.Random.Decimal(5_000, 500_000), 2) : null,
                ImpactDays = impactType == RiskImpactType.Schedule ? faker.Random.Int(5, 45) : null,
                Owner = faker.Person.FullName,
                Status = status,
                MitigationPlan = status == RiskStatus.Mitigated ? PickParagraph(ri) : null,
                IdentifiedDate = identified,
                TargetResolutionDate = targetResolution,
                ClosedDate = closedDate,
                CreatedDate = DateTime.UtcNow
            });
        }

        return risks;
    }

    private static RiskSeverity SeverityForPortfolioTarget(int riskIndex, ProjectStatus projectStatus)
    {
        // Build an approximate target severity mix across the portfolio:
        // 3 Critical, 8 High, 14 Medium, 6 Mitigated (treated as Low severity but closed).
        // We vary by project status so active/higher-risk projects carry more critical/high risks.
        if (projectStatus == ProjectStatus.Active || projectStatus == ProjectStatus.OnHold)
        {
            var poolIndex = riskIndex % 31;
            return poolIndex switch
            {
                < 3 => RiskSeverity.Critical,
                < 11 => RiskSeverity.High,
                < 25 => RiskSeverity.Medium,
                _ => RiskSeverity.Low
            };
        }

        return (riskIndex % 4) switch
        {
            0 => RiskSeverity.High,
            1 => RiskSeverity.Medium,
            2 => RiskSeverity.Low,
            _ => RiskSeverity.Medium
        };
    }

    private static RiskStatus RiskStatusForSeverity(RiskSeverity severity, Faker faker)
    {
        if (severity == RiskSeverity.Low && faker.Random.Bool(0.5f))
            return RiskStatus.Mitigated;

        return faker.Random.Bool(0.65f) ? RiskStatus.Open : RiskStatus.InProgress;
    }
}
