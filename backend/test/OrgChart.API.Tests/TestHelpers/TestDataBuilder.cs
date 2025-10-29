using OrgChart.API.Models;
using System.Text.Json;

namespace OrgChart.API.Tests.TestHelpers;

public static class TestDataBuilder
{
    public static OrgChartResponse CreateValidOrgChartResponse()
    {
        return new OrgChartResponse
        {
            Organization = new OrganizationData
            {
                Name = "Test Organization",
                Positions = new List<Position>
                {
                    CreateCeoPosition(),
                    CreateCtoPosition(),
                    CreateDeveloperPosition()
                }
            }
        };
    }

    public static OrgChartResponse CreateMinimalOrgChartResponse()
    {
        return new OrgChartResponse
        {
            Organization = new OrganizationData
            {
                Name = "Minimal Org",
                Positions = new List<Position>
                {
                    new Position
                    {
                        Id = "pos1",
                        Title = "Manager",
                        Description = "Team Manager",
                        Department = "Management",
                        Employees = new List<Employee>
                        {
                            new Employee
                            {
                                Id = "emp1",
                                Name = "Test Manager",
                                Email = "manager@test.com",
                                StartDate = "2020-01-01"
                            }
                        }
                    }
                }
            }
        };
    }

    public static OrgChartResponse CreateEmptyOrgChartResponse()
    {
        return new OrgChartResponse
        {
            Organization = new OrganizationData
            {
                Name = "Empty Organization",
                Positions = new List<Position>()
            }
        };
    }

    private static Position CreateCeoPosition()
    {
        return new Position
        {
            Id = "ceo-001",
            Title = "Chief Executive Officer",
            Description = "Leads the organization and sets strategic direction",
            Department = "Executive",
            Employees = new List<Employee>
            {
                new Employee
                {
                    Id = "emp-ceo-001",
                    Name = "John Smith",
                    Email = "john.smith@company.com",
                    StartDate = "2019-01-15",
                    Url = "https://company.com/profiles/john-smith"
                }
            },
            Url = "https://company.com/positions/ceo"
        };
    }

    private static Position CreateCtoPosition()
    {
        return new Position
        {
            Id = "cto-001",
            Title = "Chief Technology Officer",
            Description = "Oversees technology strategy and development",
            ParentPositionId = "ceo-001",
            Department = "Technology",
            Employees = new List<Employee>
            {
                new Employee
                {
                    Id = "emp-cto-001",
                    Name = "Sarah Johnson",
                    Email = "sarah.johnson@company.com",
                    StartDate = "2020-03-01",
                    Url = "https://company.com/profiles/sarah-johnson"
                }
            },
            Url = "https://company.com/positions/cto"
        };
    }

    private static Position CreateDeveloperPosition()
    {
        return new Position
        {
            Id = "dev-001",
            Title = "Senior Software Developer",
            Description = "Develops and maintains software applications",
            ParentPositionId = "cto-001",
            Department = "Technology",
            Employees = new List<Employee>
            {
                new Employee
                {
                    Id = "emp-dev-001",
                    Name = "Mike Davis",
                    Email = "mike.davis@company.com",
                    StartDate = "2021-06-15"
                },
                new Employee
                {
                    Id = "emp-dev-002",
                    Name = "Lisa Wang",
                    Email = "lisa.wang@company.com",
                    StartDate = "2022-02-01"
                }
            }
        };
    }

    public static List<Employee> CreateEmployeeList(int count)
    {
        var employees = new List<Employee>();
        for (int i = 1; i <= count; i++)
        {
            employees.Add(new Employee
            {
                Id = $"emp-{i:000}",
                Name = $"Employee {i}",
                Email = $"employee{i}@company.com",
                StartDate = DateTime.Now.AddDays(-i * 30).ToString("yyyy-MM-dd")
            });
        }
        return employees;
    }

    public static List<Position> CreatePositionHierarchy(int levels, int positionsPerLevel)
    {
        var positions = new List<Position>();
        var positionId = 1;

        for (int level = 1; level <= levels; level++)
        {
            for (int pos = 1; pos <= positionsPerLevel; pos++)
            {
                var position = new Position
                {
                    Id = $"pos-{positionId:000}",
                    Title = $"Position {positionId}",
                    Description = $"Description for position {positionId}",
                    Department = $"Department {(positionId % 3) + 1}",
                    Employees = CreateEmployeeList(1)
                };

                // Set parent for levels > 1
                if (level > 1)
                {
                    var parentIndex = (pos - 1) / 2; // Simple parent assignment
                    var parentLevel = level - 1;
                    var parentId = ((parentLevel - 1) * positionsPerLevel) + parentIndex + 1;
                    position.ParentPositionId = $"pos-{parentId:000}";
                }

                positions.Add(position);
                positionId++;
            }
        }

        return positions;
    }

    /// <summary>
    /// Loads sample data from the docs/sample-data.json file
    /// </summary>
    public static OrgChartResponse CreateSampleDataResponse()
    {
        // Path relative to the project root
        var sampleDataPath = Path.Combine("..", "..", "..", "..", "..", "docs", "sample-data.json");
        
        if (!File.Exists(sampleDataPath))
        {
            // Fallback to a simpler valid response if file not found
            return CreateValidOrgChartResponse();
        }

        var jsonContent = File.ReadAllText(sampleDataPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var response = JsonSerializer.Deserialize<OrgChartResponse>(jsonContent, options);
        return response ?? CreateValidOrgChartResponse();
    }

    /// <summary>
    /// Gets the sample data as JSON string for HTTP mocking
    /// </summary>
    public static string GetSampleDataJson()
    {
        var sampleDataPath = Path.Combine("..", "..", "..", "..", "..", "docs", "sample-data.json");
        
        if (File.Exists(sampleDataPath))
        {
            return File.ReadAllText(sampleDataPath);
        }

        // Fallback to serialized valid response
        var response = CreateValidOrgChartResponse();
        return JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}