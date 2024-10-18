//import Cleave from './Cleave.js';
import Cleave from './cleave-esm.min.js';

//¡± for date-mask in client side of MudBlazor DatePicer
/// ref¡÷[MudDatePicker with mask jump back caret on input in Server Side](https://github.com/MudBlazor/MudBlazor/issues/6796)
/// ref¡÷[CleaveMudDateMask](https://github.com/csuka1219/CleaveMudDateMask)
/// ref¡÷[Cleave.js](https://github.com/nosir/cleave.js/tree/master)
export function setCleaveMask(element /* HTMLElement */, options /* object */) {
  new Cleave(element, options);
}