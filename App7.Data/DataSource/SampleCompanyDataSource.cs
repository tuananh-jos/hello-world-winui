using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;

namespace App7.Data.DataSource;
public class SampleCompanyDataSource : ISampleCompanyDataSource
{
    public Task<List<SampleCompany>> GetAllAsync() => throw new NotImplementedException();
    public Task InsertAsync(SampleCompany company) => throw new NotImplementedException();
}
