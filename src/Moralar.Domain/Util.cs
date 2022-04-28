using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Data.Enum;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.ResidencialProperty;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using RestSharp;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.JwtMiddleware;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain
{
    public static class Util
    {
        public static List<string> AcceptedFiles = new List<string>() { ".xlsx", ".xls" };

        private static IStringLocalizer _localizer { get; set; }
        public static string GetClientIp() => Utilities.HttpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public static Dictionary<string, string> GetTemplateVariables()
        {

            var dataBody = new Dictionary<string, string>();
            try
            {
                dataBody.Add("__bg-cardbody__", "#F2F3F3");
                dataBody.Add("__bg-cardfooter__", "#A5559A");
                dataBody.Add("__cl-body__", "#000000");
                dataBody.Add("{{ baseUrl }}", $"{BaseConfig.CustomUrls[0]}content/images");
                dataBody.Add("{{ contact }}", Utilities.GetConfigurationRoot().GetSection("contactEmail").Get<string>());
                dataBody.Add("{{ appName }}", BaseConfig.ApplicationName);
            }
            catch (Exception)
            {

                //unused
            }

            return dataBody;

        }

        public static long? ToUnixCustom(this string date)
        {
            try
            {
                var dateTime = date.TryParseAnyDate();

                return new DateTimeOffset(dateTime).ToUnixTimeSeconds();

            }
            catch (Exception)
            {
                return (long?)null;
            }
        }

        /// <summary>
        ///  CONVERTER STRING EM ENUM
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToEnumCustom<T>(this string str)
        {
            var enumType = typeof(T);
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute =
                    ((EnumMemberAttribute[])enumType.GetField(name)
                        .GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                if (enumMemberAttribute.Value == str) return (T)Enum.Parse(enumType, name);
            }

            //throw exception or whatever handling you want or
            return default;
        }

        public static async Task<List<TV>> ReadAndValidationExcel<TV>(IFormFile file, int sheetIndex = 1) where TV : new()
        {
            var response = new List<TV>();
            try
            {
                using (ExcelPackage package = new ExcelPackage(file.OpenReadStream()))
                {
                    //get the first sheet from the excel file
                    ExcelWorksheet sheet = package.Workbook.Worksheets[sheetIndex];

                    var listEntityViewModel = sheet.ConvertSheetToObjects<TV>();

                    for (int i = 0; i < listEntityViewModel.Count(); i++)
                    {

                        var isInvalidState = listEntityViewModel[i].ModelIsValid(customStart: $"Erro na linha {i + 1}, verifique os dados informados");
                        if (isInvalidState != null)
                            throw new Exception(isInvalidState.Message);
                       
                    }

                    response = listEntityViewModel.ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }

            return response;
        }

        public static IList<T> ConvertSheetToObjects<T>(this ExcelWorksheet worksheet) where T : new()
        {
            Func<CustomAttributeData, bool> func = (CustomAttributeData y) => y.AttributeType == typeof(Column);
            List<ExportInfo> list = typeof(T).GetProperties().Select((PropertyInfo p, int i) => new ExportInfo
            {
                Property = p,
                Column = (p.GetCustomAttributes<Column>().FirstOrDefault()?.ColumnIndex ?? (i + 1))
            }).ToList();
            IOrderedEnumerable<int> source = from x in worksheet.Cells.Select((ExcelRangeBase cell) => cell.Start.Row).Distinct()
                                             orderby x
                                             select x;
            List<T> list2 = new List<T>();
            for (int j = 2; j <= source.Count(); j++)
            {
                T val = new T();
                bool flag = false;
                for (int k = 0; k < list.Count(); k++)
                {
                    ExportInfo exportInfo = list[k];
                    ExcelRange excelRange = worksheet.Cells[j, exportInfo.Column];
                    if (!string.IsNullOrEmpty(excelRange.GetValue<string>()) && exportInfo.SetValueCustom(val, excelRange) && !flag)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    list2.Add(val);
                }
            }

            return list2;
        }

        public static FamilyHolder SetHolder(this FamilyImportViewModel model)
        {
            var response = new FamilyHolder();

            try
            {
                response.Scholarity = model.Escolaridade.ToEnumCustom<TypeScholarity>();
                response.Birthday = model.Data_de_Nascimento.ToUnixCustom();
                response.Cpf = model.Cpf_do_titular.OnlyNumbers();                
                response.Email = string.IsNullOrEmpty(model.E_mail) ? null : model.E_mail.ToLower();
                response.Genre = model.Genero.ToEnumCustom<TypeGenre>();
                response.Name = model.Nome_do_titular;
                response.Phone = model.Telefone.OnlyNumbers();
                response.Number = model.Numero_do_cadastro;

            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
        public static FamilySpouse SetSpouse(this FamilyImportViewModel model)
        {
            var response = new FamilySpouse();

            try
            {
                response.Birthday = model.Data_de_Nascimento_do_Conjuge.ToUnixCustom();
                response.Genre = model.Genero_Conjuge.ToEnumCustom<TypeGenre>();
                response.SpouseScholarity = model.Escolaridade_Conjuge.ToEnumCustom<TypeScholarity>();
                response.Name = model.Nome_do_Conjuge;
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
        public static FamilyAddress SetAddress(this FamilyImportViewModel model)
            => new FamilyAddress();

        public static FamilyFinancial SetFinancial(this FamilyImportViewModel model)
        {
            var response = new FamilyFinancial();
            try
            {
                response.FamilyIncome = (decimal)model.Renda_Familiar.ToDouble();
                response.IncrementValue = (decimal)model.Valor_Incremento.ToDouble();
                response.MaximumPurchase = (decimal)model.Valor_para_compra_de_imovel.ToDouble();
                response.PropertyValueForDemolished = (decimal)model.Valor_imovel_demolido.ToDouble();
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
        public static FamilyPriorization SetPriorization(this FamilyImportViewModel model)
        {
            var response = new FamilyPriorization();

            try
            {
                response.WorkFront = model.Frente_de_Obras.MapPriorityRate(1);
                response.PermanentDisabled = model.Deficiencia_que_demande_imovel_acessivel.MapPriorityRate(2);
                response.ElderlyOverEighty = model.Idoso_acima_de_80_anos.MapPriorityRate(3);
                response.WomanServedByProtectiveMeasure = model.Mulher_atendida_por_medida_protetiva.MapPriorityRate(4);
                response.FemaleBreadwinner = model.Mulher_chefe_de_familia.MapPriorityRate(5);
                response.SingleParent = model.Monoparental.MapPriorityRate(6);
                response.FamilyWithMoreThanFivePeople = model.Familia_com_mais_5_pessoas.MapPriorityRate(7);
                response.ChildUnderEighteen = model.Filhos_menores_de_18_anos.MapPriorityRate(8);
                response.HeadOfHouseholdWithoutIncome = model.Chefe_de_familia_sem_renda.MapPriorityRate(9);
                response.BenefitOfContinuedProvision = model.Beneficio_de_prestacao_continuada.MapPriorityRate(10);
                response.FamilyPurse = model.Bolsa_Familia.MapPriorityRate(11);
                response.InvoluntaryCohabitation = model.Coabitacao_involuntaria.MapPriorityRate(12);
                response.FamilyIncomeOfUpTwoMinimumWages = model.Renda_familiar_de_ate_dois_salarios_minimos.MapPriorityRate(13);
                response.YearsInSextyAndSeventyNine = model.Idoso_60_ate_79_anos.MapPriorityRate(14);
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }

        public static PriorityRate MapPriorityRate(this string value, int rate)
        {
            return new PriorityRate()
            {
                Rate = rate,
                Value = value.ToBoolean()
            };
        }

        public static string MapReason(this string reason)
            => string.IsNullOrEmpty(reason) ? "" : $"Motivo: {reason}";

        public static ResidencialPropertyFeatures MapFetures(this ResidencialPropertyImportViewModel model)
        {
            var response = new ResidencialPropertyFeatures();
            try
            {

                response.TypeProperty = model.TypeProperty.ToEnum<TypeProperty>();
                response.HasAccessLadder = model.HasAccessLadder.ToBoolean();
                response.CondominiumValue = model.CondominiumValue.ToDouble();
                response.FloorLocation = model.FloorLocation.ToInt();
                response.TypeGasInstallation = model.TypeGasInstallation.ToEnum<TypePropertyGasInstallation>();
                response.HasAccessRamp = model.HasAccessRamp.ToBoolean();
                response.SquareFootage = model.SquareFootage.ToDouble();
                response.HasAdaptedToPcd = model.HasAdaptedToPcd.ToBoolean();
                response.HasCistern = model.HasCistern.ToBoolean();
                response.HasElavator = model.HasElavator.ToBoolean();
                response.PropertyValue = model.PropertyValue.ToDouble();
                response.PropertyRegularization = model.PropertyRegularization.ToEnum<TypePropertyRegularization>();
                response.NumberOfBedrooms = model.NumberOfBedrooms.ToInt();
                response.HasGarage = model.HasGarage.ToBoolean();
                response.HasServiceArea = model.HasServiceArea.ToBoolean();
                response.HasWall = model.HasWall.ToBoolean();
                response.HasYard = model.HasYard.ToBoolean();
                response.IptuValue = model.IptuValue.ToDouble();
                response.Neighborhood = model.NeighborhoodLocalization;
                response.NumberFloors = model.NumberFloors.ToInt();
                response.NumberOfBathrooms = model.NumberOfBathrooms.ToInt();

            }
            catch (Exception)
            {
                throw;
            }
            return response;
        }

        public static async Task<ResidencialPropertyAdress> MapAddress(this ResidencialPropertyImportViewModel model)
        {

            var response = new ResidencialPropertyAdress();

            try
            {
                var infoAddressByZipCode = await GetInfoZipCode(model.CEP);

                if (infoAddressByZipCode == null)
                    throw new Exception("Cep não encontrado");

                var infoLocation = Utilities.GetInfoFromAdressLocation($"{infoAddressByZipCode.StreetAddress}, {model.Number} - {infoAddressByZipCode.Neighborhood}, {infoAddressByZipCode.CityName} - {infoAddressByZipCode.StateUf}");

                response.CEP = model.CEP.OnlyNumbers();
                response.CityId = infoAddressByZipCode.CityId;
                response.CityName = infoAddressByZipCode.CityName;
                response.Neighborhood = infoAddressByZipCode.Neighborhood;
                response.Complement = model.Complement;
                response.Number = model.Number;
                response.StateId = infoAddressByZipCode.StateId;
                response.StateName = infoAddressByZipCode.StateName;
                response.StreetAddress = infoAddressByZipCode.StreetAddress;
                response.StateUf = infoAddressByZipCode.StateUf;

                if (infoLocation.Erro == false)
                {
                    response.Latitude = infoLocation.Geometry.Location.Lng;
                    response.Longitude = infoLocation.Geometry.Location.Lng;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }

        public static bool CheckHasField(string[] fields, string field)
         => fields.Count(x => x.ToLower() == field.ToLower()) > 0;

        public static async Task<InfoAddressViewModel> GetInfoZipCode(string zipCode)
        {
            try
            {
                var zipCodeFormat = zipCode?.OnlyNumbers().PadLeft(8, '0');

                if (string.IsNullOrEmpty(zipCodeFormat))
                    throw new Exception("Informe um CEP");

                var client = new RestClient("https://api.megaleios.com");
                var request = new RestRequest($"/api/v1/City/GetInfoFromZipCode/{zipCodeFormat}", Method.GET);


                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");

                var response = await client.Execute<ReturnViewModel>(request);

                if (response.StatusCode != HttpStatusCode.BadRequest && response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Ocorreu um erro ao informações do CEP {zipCode}");

                if (response.Data == null || response.Data.Erro)
                    throw new Exception(response.Data?.Message ?? $"CEP {zipCode} não encontrado");

                return JsonConvert.DeserializeObject<InfoAddressViewModel>(JsonConvert.SerializeObject(response.Data.Data));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ToReal(this decimal price, bool notAround = false)
        {
            return notAround
                ? string.Format(new CultureInfo("pt-BR"), "{0:C}", price)
                : string.Format(new CultureInfo("pt-BR"), "{0:C}", Math.Truncate(price * 100) / 100);
        }

        public static string ToReal(this double price, bool notAround = false) => notAround
                ? string.Format(new CultureInfo("pt-BR"), "{0:C}", price)
                : string.Format(new CultureInfo("pt-BR"), "{0:C}", Math.Truncate(price * 100) / 100);

        public static string MapBoolean(this bool value)
         => value == true ? "Sim" : "Não";
        public static int ToInt(this string value)
        {
            var response = 0;
            try
            {
                int.TryParse(value, out response);
            }
            catch (Exception) {/*UNUSED*/}

            return response;
        }

        public static bool ToBoolean(this string value)
            => value?.ToLower() == "sim" ? true : false;


        public static double ToDouble(this string balanceAvailableForWithdraw)
        {
            var value = 0D;
            try
            {
                var pattern = @"(R\$|BRL)";

                balanceAvailableForWithdraw = Regex.Replace(balanceAvailableForWithdraw, pattern, string.Empty).Trim();
                balanceAvailableForWithdraw = balanceAvailableForWithdraw.Replace(',', '.');

                double.TryParse(balanceAvailableForWithdraw, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

                return value;
            }
            catch (Exception)
            {
                return value;
            }
        }



        /// <summary>
        ///  EXPORT LIST ENTITY TO TABLE IN EXCEL FILE
        /// </summary>
        /// <param name="entitys"></param>
        /// <param name="path"></param>
        /// <param name="workSheetName"></param>
        /// <param name="fileName"></param>
        /// <param name="hexBColor"></param>
        /// <param name="hexTxtColor"></param>
        /// <param name="autoFit"></param>
        /// <param name="ext"></param>
        public static void ExportToExcelMultiWorksheet(string path, List<string> workSheetNames,
            string fileName = "Export", string hexBColor = null, string hexTxtColor = null, bool autoFit = true,
            string ext = ".xlsx")
        {
            var bColor = Utilities.GetColorFromHex(Color.FromArgb(68, 114, 196), hexBColor);
            var txtColor = Utilities.GetColorFromHex(Color.White, hexTxtColor);

            var sFileName = $"{fileName.Split('.')[0]}{ext}";

            #region FilePrepare

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var file = new FileInfo(Path.Combine(path, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(path, sFileName));
            }

            #endregion

            using (var package = new ExcelPackage(file))
            {

                var worksheet = package.Workbook.Worksheets.Add(workSheetNames[0]);

                var t = typeof(FamilyImportViewModel);
                var headings = t.GetProperties();
                for (var a = 0; a < headings.Count(); a++)
                {
                    int num = a + 1;
                    var name = headings[a].GetCustomAttribute<DisplayAttribute>();

                    DisplayAttribute customAttribute = headings[a].GetCustomAttribute<DisplayAttribute>();
                    
                    worksheet.Cells[1, a + 1].Value = name?.Name ?? headings[a].Name;
                    DropDownExcel customAttribute2 = headings[a].GetCustomAttribute<DropDownExcel>();

                    if (customAttribute2 == null)
                    {
                        continue;
                    }

                    Array values = Enum.GetValues(customAttribute2.Options);
                    List<string> list = new List<string>();
                    foreach (int item in values)
                    {
                        list.Add(customAttribute2.Options.GetEnumMemberValueCustom(Enum.GetName(customAttribute2.Options, item)));
                    }

                    string address1 = ExcelCellBase.GetAddress(2, num, 1048576, num);
                    IExcelDataValidationList excelDataValidationList = worksheet.DataValidations.AddListValidation(address1);
                    excelDataValidationList.ShowErrorMessage = true;
                    excelDataValidationList.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                    excelDataValidationList.ErrorTitle = "Valor inválido";
                    excelDataValidationList.Error = "Selecione um item da lista";
                    for (int j = 0; j < list.Count(); j++)
                    {
                        excelDataValidationList.Formula.Values.Add(list[j]);
                    }

                    excelDataValidationList.AllowBlank = customAttribute2.AllowBlank;
                    excelDataValidationList.Validate();

                }

                var address = worksheet.Cells[1, headings.Count()]?.Address;

                using (var rng = worksheet.Cells[$"A1:{address ?? "AD1"}"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(bColor);
                    rng.Style.Font.Color.SetColor(txtColor);
                }

                if (autoFit)
                    worksheet.Cells.AutoFitColumns();

                var worksheet2 = package.Workbook.Worksheets.Add(workSheetNames[1]);


                var t2 = typeof(FamilyMemberImportViewModel);
                var headings2 = t2.GetProperties();
                for (var a = 0; a < headings2.Count(); a++)
                {
                    int num = a + 1;
                    var name = headings2[a].GetCustomAttribute<DisplayAttribute>();                    
                    worksheet2.Cells[1, a + 1].Value = name?.Name ?? headings2[a].Name;

                    DropDownExcel customAttribute2 = headings2[a].GetCustomAttribute<DropDownExcel>();

                    if (customAttribute2 == null)
                    {
                        continue;
                    }

                    Array values = Enum.GetValues(customAttribute2.Options);
                    List<string> list = new List<string>();
                    foreach (int item in values)
                    {
                        list.Add(customAttribute2.Options.GetEnumMemberValueCustom(Enum.GetName(customAttribute2.Options, item)));
                    }

                    string address1 = ExcelCellBase.GetAddress(2, num, 1048576, num);
                    IExcelDataValidationList excelDataValidationList = worksheet2.DataValidations.AddListValidation(address1);
                    excelDataValidationList.ShowErrorMessage = true;
                    excelDataValidationList.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                    excelDataValidationList.ErrorTitle = "Valor inválido";
                    excelDataValidationList.Error = "Selecione um item da lista";
                    for (int j = 0; j < list.Count(); j++)
                    {
                        excelDataValidationList.Formula.Values.Add(list[j]);
                    }

                    excelDataValidationList.AllowBlank = customAttribute2.AllowBlank;
                    excelDataValidationList.Validate();

                }

                var address2 = worksheet2.Cells[1, headings2.Count()]?.Address;

                using (var rng = worksheet2.Cells[$"A1:{address2 ?? "AD1"}"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(bColor);
                    rng.Style.Font.Color.SetColor(txtColor);
                }

                if (autoFit)
                    worksheet2.Cells.AutoFitColumns();

                package.Save(); //Save the workbook.
            }

        }



        public static ReturnViewModel ModelIsValid<TEntity>(this TEntity entity, bool onlyValidFields = false, string[] customValidFields = null, string[] ignoredFields = null, string customStart = null) where TEntity : new()
        {
            var context = new ValidationContext(entity);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var result = Validator.TryValidateObject(entity, context, validationResults, true);

            return ModelIsValid(validationResults, onlyValidFields, customValidFields, ignoredFields, customStart);
        }

        public static ReturnViewModel ModelIsValid(List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults, bool onlyValidFields = false, string[] customValidFields = null, string[] ignoredFields = null, string startMessage = null)
        {

            if (validationResults != null && validationResults.Count() > 0)
            {
                var errors = new Dictionary<string, string>();

                for (int i = 0; i < validationResults.Count; i++)
                {

                    var validationItem = validationResults[i];
                    var memberName = validationItem.MemberNames.FirstOrDefault();

                    if (string.IsNullOrEmpty(memberName) || (ignoredFields != null && ignoredFields.Count(x => x.ContainsIgnoreCase(memberName)) > 0))
                        continue;
                }

                return new ReturnViewModel()
                {
                    Erro = true,
                    Errors = errors,
                    Message = $"{(startMessage?.TrimStart() ?? "")} {errors.FirstOrDefault().Value}".Trim()
                };
            }
            else
                return null;
        }




        public static Claim SetRole(TypeProfile typeProfile) => new Claim(ClaimTypes.Role, Enum.GetName(typeProfile.GetType(), typeProfile));
        public static Claim GetUserName(this HttpRequest httpRequest)
        {
            var userName = httpRequest.GetClaimFromToken("UserName");
            if (string.IsNullOrEmpty(userName) == false)
                return new Claim("UserName", userName);
            return null;
        }


        public static List<string> GetDiferentFields<T, TY>(this T target, TY source) where T : class
          where TY : class
        {

            var response = new List<string>();
            try
            {
                foreach (var prop in source.GetType().GetProperties())
                {
                    var targetValue = Utilities.GetValueByProperty(target, prop.Name);

                    var sourceValue = Utilities.GetValueByProperty(source, prop.Name);

                    if (Equals(targetValue, sourceValue) == false)
                        response.Add(prop.Name);
                }
            }
            catch (Exception)
            {

                /*unused*/
            }
            return response;
        }
        public static string GetFieldError<T>(int column, int line)
        {
            try
            {
                var propertyInfo = typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute<Column>().ColumnIndex == column);

                if (propertyInfo == null)
                    return $"O valor da coluna {column} na linha {line} está inválido";

                return $"O valor do campo \"{propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo.Name}\" está inválido";

            }
            catch (Exception ex)
            {

                return $"{ex.InnerException} {ex.Message}".TrimEnd();
            }
        }

        public static TypeProfile GetRole(this HttpRequest request)
        {

            var role = request.GetClaimFromToken(ClaimTypes.Role);

            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(ClaimTypes.Role), "Tipo de usuário não identificado");

            return (TypeProfile)Enum.Parse(typeof(TypeProfile), role);
        }
        public static List<SelectItemEnumViewModel> GetMembersOfEnum<T>()
        {
            try
            {
                if (typeof(T).GetTypeInfo().IsEnum == false)
                    throw new ArgumentException("Type must be an enum");

                return Enum.GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new SelectItemEnumViewModel()
                    {
                        Value = (int)(object)x,
                        Name = x.ToString(),
                    }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Language GetCurrentLocale(this HttpRequest request)
        {
            try
            {
                if (request.Headers.Keys.Count(x => x == "Accept-Language") > 0)
                {
                    return (Language)Enum.Parse(typeof(Language), request.Headers.GetValue("Accept-Language"), true);
                }
            }
            catch (Exception)
            {
                /**/
            }
            return Language.En;
        }

        public static T SetIfDifferentCustom<T, TY>(this T target, TY source) where T : class
                   where TY : class
        {
            var allProperties = source.GetType().GetProperties().ToList();

            for (int i = 0; i < allProperties.Count(); i++)
            {
                var sourceValue = Utilities.GetValueByProperty(source, allProperties[i].Name);

                // VERIFICA SE EXISTE VALOR OU ACEITA NULL
                if (Equals(sourceValue, null) == false)
                    Utilities.SetPropertyValue(target, allProperties[i].Name, sourceValue);
            }
            return target;
        }

        /// <summary>
        ///  METODO PARA OBTER OS CAMPOS QUE FORAM RECEBIDOS NO JSON  UTILIZADO EM CONJUNTO COM SETIFDIFERENT PARA SIMULAR
        ///  METODO PUT
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string[] GetFieldsFromBodyCustom(this IHttpContextAccessor httpContext)
        {
            var dic = new string[] { };
            try
            {

                if (httpContext.HttpContext.Request.Body.CanSeek)
                {
                    httpContext.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(httpContext.HttpContext.Request.Body))
                    {
                        var body = reader.ReadToEnd();
                        dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(body)
                            .Select(x => x.Key?.UppercaseFirst()).ToArray();
                    }
                }
            }
            catch (Exception)
            {
                //ignored
            }

            return dic;
        }

        public static (bool Birthday, bool Gender) HasValidMember(Family entityFamily, int startTargetAudienceAge, int endTargetAudienceAge, TypeGenre? typeGender)
        {
            try
            {
                var validGender = false;
                var listAges = new List<int>();
                var listTypeGender = new List<TypeGenre>();

                if (entityFamily.Holder.Birthday != null)
                {
                    listAges.Add(entityFamily.Holder.Birthday.Value.TimeStampToDateTime().CalculeAge());
                }

                if (entityFamily.Holder.Genre != null)
                {
                    listTypeGender.Add(entityFamily.Holder.Genre.GetValueOrDefault());
                }

                for (int i = 0; i < entityFamily.Members.Count(); i++)
                {
                    if (entityFamily.Members[i].Birthday > 0)
                        listAges.Add(entityFamily.Members[i].Birthday.TimeStampToDateTime().CalculeAge());
                }

                var validBirthday = listAges.Count(age => age >= startTargetAudienceAge && age <= endTargetAudienceAge) > 0;
                
                if (typeGender == TypeGenre.Todos)
                {
                    validGender = true;
                }
                else
                {
                    validGender = (typeGender == null || listTypeGender.Count(x => x == typeGender) > 0);
                }
                
                return (validBirthday, validGender);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public static dynamic GetPayloadPush(RouteNotification route = RouteNotification.System)
        {

            dynamic payloadPush = new JObject();

            payloadPush.route = (int)route;

            return payloadPush;

        }

        public static dynamic GetSettingsPush()
        {

            dynamic settings = new JObject();

            settings.ios_badgeType = "Increase";
            settings.ios_badgeCount = 1;
            //settings.android_channel_id = ""; /*solicitar para equipe mobile*/

            return settings;

        }


        //
        // Summary:
        //     METODO PARA OBTER O ATRIBUTO MEMBERVALUE
        //
        // Parameters:
        //   typeEnum:
        //
        //   value:
        //
        // Type parameters:
        //   T:
        //
        // Exceptions:
        //   T:System.Exception:
        public static string GetEnumMemberValueCustom<T>(this Type typeEnum, T value)
        {
            if (!typeEnum.GetTypeInfo().IsEnum)
            {
                throw new Exception("Informe typo Enum");
            }

            return typeEnum.GetTypeInfo().DeclaredMembers.SingleOrDefault((MemberInfo x) => x.Name == value.ToString())?.GetCustomAttribute<EnumMemberAttribute>(inherit: false)?.Value;
        }




    }
}
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property)]
    public class DropDownExcel : Attribute
    {
        public Type Options
        {
            get;
            set;
        }

        public bool AllowBlank
        {
            get;
            set;
        }
    }
