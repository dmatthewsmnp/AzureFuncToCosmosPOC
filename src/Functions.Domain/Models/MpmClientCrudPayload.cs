using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Domain.Models;

public record MpmClientCrudPayload(Guid ClientId,
                                int ClientNo,
                                string? ClientName,
                                string? BusinessPhone,
                                string? BusinessFax,
                                string? Website,
                                string? Street1,
                                string? Street2,
                                string? Street3,
                                string? City,
                                string? Province,
                                string? PostalCode,
                                string? Country);
