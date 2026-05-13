using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace WRR.Admin.Extension
{

    public static class SetTempDataModelStateAttribute 
    {
        public static void SerializeModelState(this Controller controller)
        {
            var errors = controller.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            controller.TempData["ModelStateErrors"] = JsonSerializer.Serialize(errors);
        }

        public static void RestoreModelState(this Controller controller)
        {
            if (controller.TempData.ContainsKey("ModelStateErrors"))
            {
                var serializedErrors = controller.TempData["ModelStateErrors"] as string;
                var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(serializedErrors);

                foreach (var (key, errorMessages) in errors)
                {
                    foreach (var errorMessage in errorMessages)
                    {
                        controller.ModelState.AddModelError(key, errorMessage);
                    }
                }
            }
            else
            {
                controller.ModelState.Clear();
            }
        }

        /* public override void OnActionExecuted(ActionExecutedContext context)
         {
             base.OnActionExecuted(context);

             if (context.Controller is Controller controller)
             {
                 // Store ModelState in TempData
                 controller.TempData["ModelState"] = controller.ViewData.ModelState;
             }
         }*/


    }




}
