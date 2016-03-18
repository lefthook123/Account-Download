using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Account_Download.ARPartnerAPI;
using System.Configuration;
using System.Web.Services.Protocols;
using System.IO;
using System.Reflection;
using System.Data.Entity.Validation;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace Account_Download
{
    public static class Utilities
    {
        public static string username = ConfigurationSettings.AppSettings["userName"];
        public static string password = ConfigurationSettings.AppSettings["password"];
        public static string logFolder { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\logs"; } }
 


        public  static SforceService binding = null;

        public static bool login()
        {
            LoginResult lr;
            binding = new SforceService();
            try
            {
                lr = binding.login(username, password);
            }
            catch(SoapException e)
            {
                writeToLog(e.Code.ToString());
                writeToLog("An unexpected error has occurred: " + e.Message);
                writeToLog(e.StackTrace);
                return false;
            }
            if(lr.passwordExpired)
            {
                writeToLog("An error has occurred. Your password has expired.");
                return false;
            }
            binding.Url = lr.serverUrl;
            binding.SessionHeaderValue = new SessionHeader();
            binding.SessionHeaderValue.sessionId = lr.sessionId;
            return true;
        }


        public static void AccountDonwload()
        {
            String accountquery = ConfigurationSettings.AppSettings["accountquery"];
            decimal progess = 0;
            ARSalesforceEntities arsf = new ARSalesforceEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)arsf).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [ARAccounts]");
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0,0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Accounts: " + queryResult.size);
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Account: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARAccount aracct = new ARAccount();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARAccount).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t=araccounttypeinfo.PropertyType;
                                object d;
                                String s= sobj.Any[i].InnerText;
                                if(t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                   
                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }else{d=Convert.ChangeType(s,t);}
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        arsf.ARAccounts.Add(aracct);
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                throw e;
            }



        }
        public static void ContactDonwload()
        {
            String contactquery = ConfigurationSettings.AppSettings["contactquery"];
            decimal progess = 0;
            ARSalesforceEntities1 arsf = new ARSalesforceEntities1();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)arsf).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [ARContacts]");
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(contactquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Contacts: " + queryResult.size);
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Contact: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARContact aracct = new ARContact();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARContact).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        arsf.ARContacts.Add(aracct);
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                throw e;
            }



        }
        public static void OpportunityDonwload()
        {
            String contactquery = ConfigurationSettings.AppSettings["opportunityquery"];
            decimal progess = 0;
            AROpportunityEntities arsf = new AROpportunityEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)arsf).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [AROpportunities]");
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(contactquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Opportunities: " + queryResult.size);
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Opportunity: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        AROpportunity aracct = new AROpportunity();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(AROpportunity).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        arsf.AROpportunities.Add(aracct);
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                throw e;
            }



        }
        public static void TaskDonwload()
        {
            String contactquery = ConfigurationSettings.AppSettings["taskquery"];
            decimal progess = 0;
            ARTaskEntities arsf = new ARTaskEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)arsf).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [ARTasks]");
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(contactquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Tasks: " + queryResult.size);
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Tasks: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARTask aracct = new ARTask();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARTask).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        arsf.ARTasks.Add(aracct);
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                throw e;
            }



        }

        public static void UserDonwload()
        {
            String contactquery = ConfigurationSettings.AppSettings["userquery"];
            decimal progess = 0;
            ARUserEntities arsf = new ARUserEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)arsf).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [ARUsers]");
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(contactquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Tasks: " + queryResult.size);
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Tasks: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARUser aracct = new ARUser();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARUser).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        arsf.ARUsers.Add(aracct);
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                throw e;
            }



        }

        public static void updateAccountRecords()
        {
            String accountquery = ConfigurationSettings.AppSettings["accountupdatequery"];
            decimal progess = 0;
            ARSalesforceEntities arsf = new ARSalesforceEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Accounts: " + queryResult.size);
                writeToLog(String.Format("Total Accounts Upserted: {0}", queryResult.size));
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    if (records == null)
                    {
                        return;
                    }
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 5);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Account: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARAccount aracct = new ARAccount();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARAccount).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        if (arsf.ARAccounts.Any(a => a.Id == sobj.Id))
                        {
                            arsf.ARAccounts.Attach(aracct);
                            arsf.Entry(aracct).State = EntityState.Modified;
                            writeToLog(String.Format("Account Updated: {0}", aracct.Id));
                            
                        }
                        else
                        {
                            arsf.ARAccounts.Add(aracct);

                             writeToLog(String.Format("Account Inserted: {0}", aracct.Id));
                            
                        }
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;
                writeToLog(updateException.Message);
                writeToLog(sqlException.Message);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                writeToLog(e.StackTrace);
                throw e;
            }
            
        }
        public static void updateContactRecords()
        {
            String accountquery = ConfigurationSettings.AppSettings["contactupdatequery"];
            decimal progess = 0;
            ARSalesforceEntities1 arsf = new ARSalesforceEntities1();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Contacts: " + queryResult.size);
                writeToLog(String.Format("Total Contacts Upserted: {0}", queryResult.size));
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    if (records == null)
                    {
                        return;
                    }
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 6);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Contact: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARContact aracct = new ARContact();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARContact).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        if (arsf.ARContacts.Any(a => a.Id == sobj.Id))
                        {
                            arsf.ARContacts.Attach(aracct);
                            arsf.Entry(aracct).State = EntityState.Modified;
                            writeToLog(String.Format("Contact Updated: {0}", aracct.Id));

                        }
                        else
                        {
                            arsf.ARContacts.Add(aracct);

                            writeToLog(String.Format("Contact Inserted: {0}", aracct.Id));

                        }
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                writeToLog(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;
                writeToLog(updateException.Message);
                writeToLog(sqlException.Message);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                writeToLog(e.StackTrace);
                throw e;
            }

        }
        public static void updateOpportunityRecords()
        {
            String accountquery = ConfigurationSettings.AppSettings["opportunityupdatequery"];
            decimal progess = 0;
            AROpportunityEntities arsf = new AROpportunityEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Opportunities: " + queryResult.size);
                writeToLog(String.Format("Total Opportunities Upserted: {0}", queryResult.size));
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    if (records == null)
                    {
                        return;
                    }
                    count += records.Length;
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 7);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Opportunity: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        AROpportunity aracct = new AROpportunity();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(AROpportunity).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        if (arsf.AROpportunities.Any(a => a.Id == sobj.Id))
                        {
                            arsf.AROpportunities.Attach(aracct);
                            arsf.Entry(aracct).State = EntityState.Modified;
                            writeToLog(String.Format("Opportunity Updated: {0}", aracct.Id));

                        }
                        else
                        {
                            arsf.AROpportunities.Add(aracct);

                            writeToLog(String.Format("Opportunity Inserted: {0}", aracct.Id));

                        }
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;
                writeToLog(updateException.Message);
                writeToLog(sqlException.Message);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                writeToLog(e.StackTrace);
                throw e;
            }

        }
        public static void updateTaskRecords()
        {
            String accountquery = ConfigurationSettings.AppSettings["taskupdatequery"];
            decimal progess = 0;
            ARTaskEntities arsf = new ARTaskEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Tasks: " + queryResult.size);
                writeToLog(String.Format("Total Tasks Upserted: {0}", queryResult.size));
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    if (records == null)
                    {
                        return;
                    }
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 4);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Task: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARTask aracct = new ARTask();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARTask).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        if (arsf.ARTasks.Any(a => a.Id == sobj.Id))
                        {
                            arsf.ARTasks.Attach(aracct);
                            arsf.Entry(aracct).State = EntityState.Modified;
                            writeToLog(String.Format("Task Updated: {0}", aracct.Id));

                        }
                        else
                        {
                            arsf.ARTasks.Add(aracct);

                            writeToLog(String.Format("Task Inserted: {0}", aracct.Id));

                        }
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;
                writeToLog(updateException.Message);
                writeToLog(sqlException.Message);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                writeToLog(e.StackTrace);
                throw e;
            }

        }
        public static void updateUserRecords()
        {
            String accountquery = ConfigurationSettings.AppSettings["userupdatequery"];
            decimal progess = 0;
            ARUserEntities arsf = new ARUserEntities();
            arsf.Configuration.AutoDetectChangesEnabled = false;
            try
            {
                QueryResult queryResult = null;
                binding.QueryOptionsValue = new QueryOptions();
                binding.QueryOptionsValue.batchSize = 2000;
                binding.QueryOptionsValue.batchSizeSpecified = true;
                queryResult = binding.query(accountquery);
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.WriteLine("Total Users: " + queryResult.size);
                writeToLog(String.Format("Total Users Upserted: {0}", queryResult.size));
                int total = queryResult.size;
                int count = 0;
                bool done = false;
                while (!done)
                {
                    sObject[] records = queryResult.records;
                    count += records.Length;
                    if (records == null)
                    {
                        return;
                    }
                    progess = Convert.ToDecimal(count) / Convert.ToDecimal(total) * 100;
                    Console.SetCursorPosition(0, 8);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("User: " + progess.ToString("#.##") + "%" + "-----" + count);
                    foreach (sObject sobj in records)
                    {
                        ARUser aracct = new ARUser();
                        for (int i = 0; i < sobj.Any.Count(); i++)
                        {
                            PropertyInfo araccounttypeinfo = typeof(ARUser).GetProperty(sobj.Any[i].LocalName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                            if (araccounttypeinfo != null)
                            {
                                //Console.WriteLine(propertyinfo.Name);
                                //Type t = araccounttypeinfo.GetType();
                                Type t = araccounttypeinfo.PropertyType;
                                object d;
                                String s = sobj.Any[i].InnerText;
                                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {

                                    if (String.IsNullOrEmpty(s))
                                        d = null;

                                    else
                                        d = Convert.ChangeType(s, t.GetGenericArguments()[0]);
                                }
                                else { d = Convert.ChangeType(s, t); }
                                araccounttypeinfo.SetValue(aracct, d);
                            }
                        }
                        if (arsf.ARUsers.Any(a => a.Id == sobj.Id))
                        {
                            arsf.ARUsers.Attach(aracct);
                            arsf.Entry(aracct).State = EntityState.Modified;
                            writeToLog(String.Format("User Updated: {0}", aracct.Id));

                        }
                        else
                        {
                            arsf.ARUsers.Add(aracct);

                            writeToLog(String.Format("User Inserted: {0}", aracct.Id));

                        }
                    }
                    arsf.SaveChanges();
                    if (queryResult.done)
                    {
                        done = true;
                    }
                    else
                    {
                        queryResult = binding.queryMore(queryResult.queryLocator);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                UpdateException updateException = (UpdateException)ex.InnerException;
                SqlException sqlException = (SqlException)updateException.InnerException;
                writeToLog(updateException.Message);
                writeToLog(sqlException.Message);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    writeToLog(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
            eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        writeToLog(String.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage));
                    }
                }
                throw;
            }
            catch (ArgumentException e)
            {
                writeToLog(e.Message);
                // Console.WriteLine(watchpropertyinfo.Name);
                throw e;
            }

            catch (Exception e)
            {
                writeToLog(e.Message);
                writeToLog(e.StackTrace);
                throw e;
            }

        }

        public static void writeToLog(String line)
        {
            String fileName = Path.Combine(logFolder,DateTime.Now.ToString("dd-MM-yyyy")+".txt");
            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(line);
            }
        }
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }

}
